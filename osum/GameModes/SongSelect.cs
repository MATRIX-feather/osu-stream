using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using osum.Audio;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.GameModes.SongSelect;
using OpenTK.Graphics;
using osum.GameModes.Play.Components;
using osum.Graphics.Drawables;
using osum.GameplayElements;
using System.Threading;

namespace osum.GameModes
{
    enum SelectState
    {
        SongSelect,
        DifficultySelect,
        LoadingPreview,
        RankingDisplay,
        Starting
    }

    public partial class SongSelectMode : GameMode
    {
        private const string BEATMAP_DIRECTORY = "Beatmaps";
        private static List<Beatmap> availableMaps;
        private readonly List<BeatmapPanel> panels = new List<BeatmapPanel>();

        private float offset;
        private float offset_min { get { return panels.Count * -70 + GameBase.BaseSize.Height - s_Header.DrawHeight; } }
        private float offset_max = 0;

        private float velocity;

        SelectState State;

        /// <summary>
        /// Offset bound to visible limits.
        /// </summary>
        private float offsetBound
        {
            get
            {
                return Math.Min(offset_max, Math.Max(offset_min, offset));
            }
        }

        private pSprite s_Header;
        private pSprite s_Footer;
        private BeatmapPanel SelectedPanel;

        internal override void Initialize()
        {
            InitializeBeatmaps();

            Player.SetDifficulty(Difficulty.Normal);

            InputManager.OnMove += InputManager_OnMove;

            InitializeBgm();

            s_Header = new pSprite(TextureManager.Load(OsuTexture.songselect_header), new Vector2(0, 0));
            s_Header.Transform(new Transformation(new Vector2(-60, 0), Vector2.Zero, 0, 500, EasingTypes.In));
            s_Header.Transform(new Transformation(TransformationType.Rotation, -0.06f, 0, 0, 500, EasingTypes.In));
            spriteManager.Add(s_Header);

            s_Footer = new pSprite(TextureManager.Load(OsuTexture.songselect_footer), FieldTypes.StandardSnapBottomLeft, OriginTypes.BottomLeft, ClockTypes.Mode, new Vector2(0, -100), 1, true, Color4.White);
            s_Footer.OnClick += onStartButtonPressed;
            spriteManager.Add(s_Footer);
        }

        private void InitializeBeatmaps()
        {
            availableMaps = new List<Beatmap>();

            int index = 0;

#if iOS
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            
            foreach (string s in Directory.GetFiles(docs,"*.osz2"))
            {
                Beatmap reader = new Beatmap(s);

                string[] files = reader.Package == null ? new string[]{s} : reader.Package.MapFiles;
                foreach (string file in files)
                {
                    Beatmap b = new Beatmap(s);
                    b.BeatmapFilename = Path.GetFileName(file);

                    BeatmapPanel panel = new BeatmapPanel(b, this, index++);
                    spriteManager.Add(panel);

                    availableMaps.Add(b);
                    panels.Add(panel);
                }
            }
#endif

            if (Directory.Exists(BEATMAP_DIRECTORY))
                foreach (string s in Directory.GetFiles(BEATMAP_DIRECTORY))
                {
                    Beatmap reader = new Beatmap(s);

                    string[] files = reader.Package == null ? Directory.GetFiles(s, "*.osc") : reader.Package.MapFiles;
                    foreach (string file in files)
                    {
                        Beatmap b = new Beatmap(s);
                        b.BeatmapFilename = Path.GetFileName(file);

                        BeatmapPanel panel = new BeatmapPanel(b, this, index++);
                        spriteManager.Add(panel);

                        availableMaps.Add(b);
                        panels.Add(panel);
                    }
                }
        }

        private void InitializeBgm()
        {
            //Start playing song select BGM.
#if iOS
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.m4a"), true);
#else
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.mp3"), true);
#endif
            AudioEngine.Music.Play();
        }

        /// <summary>
        /// Called when a panel has been selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal void onSongSelected(object sender, EventArgs args)
        {
            BeatmapPanel panel = sender as BeatmapPanel;
            if (panel == null || State != SelectState.SongSelect) return;

            Player.SetBeatmap(panel.Beatmap);

            SelectedPanel = panel;
            State = SelectState.LoadingPreview;

            foreach (BeatmapPanel p in panels)
            {
                p.s_BackingPlate.HandleInput = false;

                if (p == panel) continue;

                foreach (pDrawable s in p.Sprites)
                {
                    //s.MoveTo(s.Position + new Vector2(-400, 0), 500, EasingTypes.InDouble);
                    s.FadeOut(100);
                }
            }

            panel.s_BackingPlate.FlashColour(Color4.White, 500);

            GameBase.Scheduler.Add(delegate
            {
                AudioEngine.Music.Load(panel.Beatmap.GetFileBytes(panel.Beatmap.AudioFilename), false);
                AudioEngine.Music.Play();
                AudioEngine.Music.Volume = 0;
                AudioEngine.Music.SeekTo(30000);

                GameBase.Scheduler.Add(showDifficultySelection, true);
            }, 400);
        }

        public override void Dispose()
        {
            base.Dispose();

            InputManager.OnMove -= InputManager_OnMove;
        }

        private void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (InputManager.IsPressed)
            {
                float change = InputManager.PrimaryTrackingPoint.WindowDelta.Y;
                float bound = offsetBound;

                if ((offset - bound < 0 && change < 0) || (offset - bound > 0 && change > 0))
                    change *= Math.Min(1, 10 / Math.Max(0.1f, Math.Abs(offset - bound)));
                offset = offset + change;
                velocity = change;
            }
        }

        public override void Update()
        {
            base.Update();

            switch (State)
            {
                case SelectState.SongSelect:
                    if (!InputManager.IsPressed)
                    {
                        float bound = offsetBound;

                        if (offset != bound)
                            velocity = 0;

                        offset = offset * 0.8f + bound * 0.2f + velocity;
                        velocity *= 0.9f;
                    }

                    if (Director.PendingMode == OsuMode.Unknown)
                    {
                        Vector2 pos = new Vector2(0, 60 + offset);
                        foreach (BeatmapPanel p in panels)
                        {
                            p.MoveTo(pos);
                            pos.Y += 70;
                        }
                    }
                    break;
                case SelectState.LoadingPreview:
                    if (AudioEngine.Music.Volume > 0)
                        AudioEngine.Music.Volume -= 0.05f;
                    break;
                case SelectState.RankingDisplay:
                case SelectState.DifficultySelect:
                    if (AudioEngine.Music.Volume < 1)
                        AudioEngine.Music.Volume += 0.005f;
                    break;
            }
        }
    }
}