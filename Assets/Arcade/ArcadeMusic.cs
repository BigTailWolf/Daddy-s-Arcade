using System;
using UnityEngine;

namespace DaddysArcade
{
    // Original, synthesized arcade music. No recordings or external music assets.
    public sealed class ArcadeMusic : MonoBehaviour
    {
        const string Preference = "DaddysArcade.MusicEnabled";
        AudioSource menu, play;
        bool gameplay, ducked, focused = true;
        public bool MusicEnabled { get; private set; }
        void Awake()
        {
            MusicEnabled = PlayerPrefs.GetInt(Preference, 1) != 0;
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            menu = CreateSource("Arcade - Welcome Home", false);
            play = CreateSource("Blocks - Side by Side", true);
            double start = AudioSettings.dspTime + .1;
            menu.PlayScheduled(start); play.PlayScheduled(start);
        }
        AudioSource CreateSource(string title, bool energetic)
        {
            var samples = Compose(energetic);
            var clip = AudioClip.Create(title, samples.Length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip; source.loop = true; source.playOnAwake = false;
            source.spatialBlend = 0; source.volume = 0;
            return source;
        }
        public void SetContext(bool inGame, bool paused)
        {
            gameplay = inGame; ducked = paused;
        }
        public void Toggle()
        {
            MusicEnabled = !MusicEnabled;
            PlayerPrefs.SetInt(Preference, MusicEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        void Update()
        {
            float volume = MusicEnabled && focused ? (ducked ? .075f : .23f) : 0;
            float fade = Time.unscaledDeltaTime * .35f;
            menu.volume = Mathf.MoveTowards(menu.volume, gameplay ? 0 : volume, fade);
            play.volume = Mathf.MoveTowards(play.volume, gameplay ? volume : 0, fade);
        }
        void OnApplicationFocus(bool hasFocus) { focused = hasFocus; }
        void OnDestroy()
        {
            if (menu != null) { menu.Stop(); Destroy(menu.clip); }
            if (play != null) { play.Stop(); Destroy(play.clip); }
        }

        public const int SampleRate = 22050;
        public static float[] Compose(bool energetic)
        {
            double beat = 60.0 / (energetic ? 112 : 96);
            var samples = new float[(int)Math.Round(16 * 4 * beat * SampleRate)];
            // Cmaj7, Am7, Fmaj7, G6, Em7, Am7, Dm7, G6; two phrases.
            int[][] chords = {
                new[]{48,52,55,59}, new[]{45,48,52,55}, new[]{41,45,48,52}, new[]{43,47,50,52},
                new[]{40,43,47,50}, new[]{45,48,52,55}, new[]{38,41,45,48}, new[]{43,47,50,52}
            };
            int[][] melody = {
                new[]{76,79,74,76,71,74,72,-1}, new[]{72,76,79,-1,76,74,72,71},
                new[]{69,72,76,79,76,-1,72,69}, new[]{71,74,76,-1,74,71,69,67},
                new[]{71,74,79,78,74,-1,71,67}, new[]{72,71,69,76,74,72,-1,69},
                new[]{69,72,77,76,72,-1,69,65}, new[]{67,71,74,76,74,71,72,-1}
            };
            var noise = new System.Random(90210);
            for (int bar=0;bar<16;bar++) {
                int phrase = bar % 8;
                var chord = chords[phrase];
                double start = bar * 4 * beat;
                foreach (int note in chord) Note(samples,start,3.8*beat,note+12,.035,0);
                for (int step=0;step<8;step++) {
                    double at = start + step * .5 * beat;
                    Note(samples,at,.36*beat,chord[(step*3+bar)%4]+24,energetic ? .075 : .045,1);
                    int lead = melody[phrase][step];
                    if (lead >= 0 && (energetic || step%2 == 0))
                        Note(samples,at,(energetic ? .43 : .85)*beat,lead+(bar>=8 && step==4 ? 12 : 0),.095,1);
                    if (step%2==0) Note(samples,at,.75*beat,chord[step==4 ? 2 : 0]-12,.12,0);
                    if (energetic || step%2==0) Percussion(samples,at,step,noise,energetic ? 1 : .55);
                }
            }
            // Fixed headroom; preserve dynamics without clipping or a limiter.
            float peak = 0;
            foreach (float sample in samples) peak = Math.Max(peak, Math.Abs(sample));
            float gain = peak > 0 ? .72f/peak : 0;
            for (int i=0;i<samples.Length;i++) samples[i] *= gain;
            return samples;
        }
        static void Note(float[] data, double start, double duration, int midi, double level, int timbre)
        {
            double frequency = 440 * Math.Pow(2,(midi-69)/12.0);
            int offset = (int)Math.Round(start*SampleRate), length = (int)(duration*SampleRate);
            for (int i=0;i<length;i++) {
                double t = i/(double)SampleRate;
                double envelope = Math.Min(1,t/.012) * Math.Min(1,(duration-t)/.06);
                double phase = 2*Math.PI*frequency*t;
                double wave = Math.Sin(phase);
                if (timbre==1) { wave = .8*wave+.16*Math.Sin(phase*2)+.04*Math.Sin(phase*3); envelope *= Math.Exp(-3*t/duration); }
                data[(offset+i)%data.Length] += (float)(level*envelope*wave);
            }
        }
        static void Percussion(float[] data, double start, int step, System.Random random, double level)
        {
            int offset = (int)Math.Round(start*SampleRate);
            for (int i=0;i<(int)(.16*SampleRate);i++) {
                double t = i/(double)SampleRate;
                double hit = (random.NextDouble()*2-1)*Math.Exp(-t*85)*.025;
                if (step%4==0) hit += Math.Sin(2*Math.PI*(52*t+.85*(1-Math.Exp(-t*35))))*Math.Exp(-t*28)*.13;
                if (step%4==2) hit += (random.NextDouble()*2-1)*Math.Exp(-t*38)*.055;
                data[(offset+i)%data.Length] += (float)(hit * Math.Min(1,t/.003)*level);
            }
        }
    }
}
