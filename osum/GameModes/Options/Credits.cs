using System;
using osum.GameplayElements;
using osum.Audio;
using osum.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using osum.Graphics.Renderers;
using osum.GameModes.SongSelect;

namespace osum.GameModes.Options
{
    public class Credits : Player
    {
        string[] creditsRoll = new string[] {
            "*osu! stream",
            "*Project Management / Overall Design",
            "Dean Herbert (peppy)",
            "*Art Concept",
            "Koko Ban",
            "*Graphics",
            "Koko Ban - Interface designs, colour scheme",
            "LuigiHann - Gameplay elements, rank letters",
            "*Implementation",
            "mm201 - Slider drawing, gameplay mechanic tweaking",
            "Intermezzo - Android platform management, file format",
            "Echo49 - file format, general tweaking",
            "*Music",
            "kodex - osu! stream theme music",
            "Natteke - credit screen mix",
            "Various Artists - level music made by respective artists",
            "*Localisation",
            "SiRiRu & co. - Japanese localisation",
            "*Correspondence",
            "dvorak - Contacting Japanese artists",
            "jericho2442 - Finding many more artists",
            "*Level Design",
            "Mapping Team - Huge thanks to all those who helped shape osu!stream mapping standards.",
            "*Thanks",
            "Nintendo (iNiS) - Ouendan gameplay concept, original level design style",
            "Nuudles - Developing the cydia osu! release which is still standing strong",
            "Testers - Special thanks to Doddler, dvorak, James, kodex, Saphier, tim0liver and my mum",
            "#bat - For all the support and help on various occasions",
            "*Note",
            "Missed People - If your name isn't on this list and you think it should be, tell peppy quickly!"
        };

        int beatLength = 800;

        public override void Initialize()
        {
            Difficulty = Difficulty.None;
            Beatmap = null;

            Clock.ResetManual();

            InitializeBgm();

            base.Initialize();

            s_ButtonBack = new BackButton(delegate { Director.ChangeMode(OsuMode.Options); });
            spriteManager.Add(s_ButtonBack);

            int time = Clock.ModeTime;

            const int speed = 17000;
            const int spacing = 1800;
            const int header_spacing = 2100;

            int len = creditsRoll.Length;
            for (int i = 0; i < len; i++)
            {
                string drawString = creditsRoll[i];

                if (drawString.Length == 0)
                    continue;

                bool isHeader = false;
                if (drawString[0] == '*')
                {
                    drawString = drawString.Substring(1);
                    isHeader = true;
                }

                pText text;

                if (isHeader)
                {

                    text = new pText(drawString, 30, Vector2.Zero, SpriteManager.drawOrderFwdPrio(i), true, new Color4(255,168,0,255))
                    {
                        Field = FieldTypes.StandardSnapTopCentre,
                        Origin = OriginTypes.Centre,
                        Clocking = ClockTypes.Manual,
                        RemoveOldTransformations = false,
                        Alpha = 1
                    };

                    if (i > 0)
                        time += header_spacing - spacing;

                    text.Transform(new Transformation(new Vector2(text.Position.X, GameBase.BaseSize.Height + 60), new Vector2(0, -100), time, time + speed));
                    time += header_spacing;
                }
                else
                {
                    string[] split = drawString.Split('-');

                    if (split.Length == 1)
                    {
                        text = new pText(drawString, 26, Vector2.Zero, SpriteManager.drawOrderFwdPrio(i), true, Color4.White)
                        {
                            Field = FieldTypes.StandardSnapTopCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Manual,
                            RemoveOldTransformations = false,
                            TextShadow = true,
                            Alpha = 1
                        };
                    }
                    else
                    {
                        text = new pText(split[0].Trim(), 24, new Vector2(-10, 0), SpriteManager.drawOrderFwdPrio(i), true, i % 2 == 0 ? new Color4(187, 230, 255, 255) : new Color4(255, 187, 253, 255))
                        {
                            Field = FieldTypes.StandardSnapTopCentre,
                            Origin = OriginTypes.CentreRight,
                            Clocking = ClockTypes.Manual,
                            RemoveOldTransformations = false,
                            Alpha = 1
                        };

                        pText text2 = new pText(split[1].Trim(), 16, new Vector2(10, 0), new Vector2(300,0), SpriteManager.drawOrderFwdPrio(i), true, i % 2 == 0 ? new Color4(200,200,200,255) : new Color4(240,240,240,255), false)
                        {
                            Field = FieldTypes.StandardSnapTopCentre,
                            Origin = OriginTypes.CentreLeft,
                            Clocking = ClockTypes.Manual,
                            RemoveOldTransformations = false,
                            Alpha = 1
                        };

                        text2.Transform(new Transformation(new Vector2(text2.Position.X, GameBase.BaseSize.Height + 60), new Vector2(text2.Position.X, -100), time, time + speed));
                        spriteManager.Add(text2);
                    }

                    text.Transform(new Transformation(new Vector2(text.Position.X, GameBase.BaseSize.Height + 60), new Vector2(text.Position.X, -100), time, time + speed));
                    time += spacing;
                }

                spriteManager.Add(text);
            }

            InputManager.OnMove += new InputHandler(InputManager_OnMove);
        }

        public override void Dispose()
        {
            InputManager.OnMove -= new InputHandler(InputManager_OnMove);
            base.Dispose();
        }

        float incrementalSpeed = 1;
        private BackButton s_ButtonBack;

        void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (!InputManager.IsPressed)
                return;

            incrementalSpeed = (-trackingPoint.WindowDelta.Y * 2) * 0.5f + incrementalSpeed * 0.5f;

            Clock.IncrementManual(incrementalSpeed);
        }

        protected override void initializeUIElements()
        {

        }

        /// <summary>
        /// Initializes the song select BGM and starts playing. Static for now so it can be triggered from anywhere.
        /// </summary>
        internal void InitializeBgm()
        {
            //Start playing song select BGM.
#if iOS
            AudioEngine.Music.Load("Skins/Default/credits.m4a", true);
#else
            AudioEngine.Music.Load("Skins/Default/credits.mp3", true);
#endif
            AudioEngine.Music.Play();
        }

        int lastBeat;
        int lastBeatNoLoop;
        public override void Update()
        {
            int currentBeat = (int)((Clock.AudioTime - 110) / (beatLength / 4f)) % 16;
            int currentBeatNoLoop = (int)((Clock.AudioTime - 110) / (beatLength / 4f));

            if (currentBeat != lastBeat)
            {
                switch (currentBeat)
                {
                    case 0:
                        spriteManager.ScaleScalar = 1.04f;
                        spriteManager.ScaleTo(1, 200, EasingTypes.In);
                        break;
                    case 2:
                    case 10:
                        spriteManager.Rotation = 0.015f;
                        spriteManager.RotateTo(0, 300);
                        break;
                    case 6:
                    case 14:
                        spriteManager.Rotation = -0.015f;
                        spriteManager.RotateTo(0, 300);
                        break;
                    case 7:
                        spriteManager.ScaleScalar = 1.01f;
                        spriteManager.ScaleTo(1, 200, EasingTypes.In);
                        break;
                    case 8:
                        spriteManager.ScaleScalar = 1.04f;
                        spriteManager.ScaleTo(1, 200, EasingTypes.In);
                        break;
                    
                }
                lastBeat = currentBeat;
            }

            if (currentBeatNoLoop != lastBeatNoLoop)
            {
                switch (currentBeatNoLoop)
                {
                    case 160:
                    case 168:
                        playfieldBackground.FlashColour(Color4.White, 500);
                        break;
                }
                Console.WriteLine(currentBeatNoLoop);
                lastBeatNoLoop = currentBeatNoLoop;
            }

            
            if (!InputManager.IsPressed)
            {
                incrementalSpeed = 0.2f + incrementalSpeed * 0.8f;
                Clock.IncrementManual(incrementalSpeed);
            }

            playfieldBackground.Velocity = 0.4f * incrementalSpeed;
            if (InputManager.IsPressed)
                incrementalSpeed = 0;

            base.Update();
        }
    }
}

