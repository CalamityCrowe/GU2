using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

enum GameState
{
    Paused,
    Playing,
    GameOver,
    StartScreen,
    Controls,
    Load
}

enum LEVEL
{
    TestArea,
    Level1,
    Level2,
    Level3,
    Level4,
    Level5,
    Boss1
}
namespace TwistedSoul
{


    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {

        //this is for doing the sprite effect on the text
        Effect textEffect;
        float effectoffset;
        bool Change = false;

        private SoundEffect MenuSound;

        private List<SoundEffect> Sounds; // this will be loaded into the player class and will contain all the projectle sounds

        int selection = 0;

        private readonly string FileName = "PlayerProgress.lst";

        static SpriteFont Menu;

        GraphicsDeviceManager graphics;
        //this sets the game state
        GameState CurrentGameState = GameState.StartScreen;
        LEVEL CurrentLevel = LEVEL.TestArea;

        Song CurrentSound;

        SpriteBatch spriteBatch;
        GamePadState NewPad, OldPad;
        // this will hold all the enemies in the game
        List<Enemies> EnemyList;

        private List<Texture2D> Projectiles_Textures;


        Map CurrentMap;// this will be used for sending the current map to the player,enemy and drawing


        private List<backgrounds> ForestScenes, CastleInterior, DayForest, MenuBackground, CurrentBack;

        private List<HealthUpgrades> hUpgrades;

        private Player Player;
        private SpriteFont HudFont;

        private Rectangle ScreenBounds;

        private int[] BossTiles;
        int Size;

        private SaveManager saveManager;

        private Rectangle ExitPoint;

        public static Random RNG = new Random();

#if DEBUG
        public static Texture2D debugPixel;
        public static SpriteFont debugFont;
#endif
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
                GraphicsProfile = GraphicsProfile.HiDef,
                SynchronizeWithVerticalRetrace = false
            };
            Window.Position = new Point(0, 0);
            Window.IsBorderless = true;


            //maps will be declared manually like this as they will load in their values from an xml file 


            Content.RootDirectory = "Content";
        }


        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            

            //this makes it so the textures dont have to passed into the class so you don't have to overload the parameters in a class
            //
            CollisionTiles.Content = this.Content;
            Particle.Content = this.Content;

            effectoffset = 0f;

            ScreenBounds = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            ForestScenes = new List<backgrounds>();
            CastleInterior = new List<backgrounds>();
            DayForest = new List<backgrounds>();
            MenuBackground = new List<backgrounds>();

            hUpgrades = new List<HealthUpgrades>();

            Projectiles_Textures = new List<Texture2D>();
            EnemyList = new List<Enemies>();
            Sounds = new List<SoundEffect>();

            HudFont = Content.Load<SpriteFont>("Hud-Elements/HudFont");
            BossTiles = new int[2];
            Size = 64;

            //this is used to have the boss arena fill the screen using the .BossGenerate method for the map
            BossTiles[0] = graphics.PreferredBackBufferWidth / Size;
            BossTiles[1] = graphics.PreferredBackBufferHeight / Size;

            saveManager = new SaveManager(FileName);

            // this sets up all the backgrounds along with the correct speed for them.
            float tempSpeed = .75f;
            for (int i = 11; i > 0; i--)
            {

                ForestScenes.Add(new backgrounds(Content.Load<Texture2D>("Backgrounds/" + i), 0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, tempSpeed));
                tempSpeed += 0.05f;
            }
            tempSpeed = .75f;
            for (int i = 0; i < 7; i++)
            {

                CastleInterior.Add(new backgrounds(Content.Load<Texture2D>("Backgrounds/Castle_Interior/" + i), 0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, tempSpeed));
                if (i < 5)
                {
                    tempSpeed += 0.05f;

                }
            }
            tempSpeed = .75f;
            for (int i = 0; i < 4; i++)
            {

                DayForest.Add(new backgrounds(Content.Load<Texture2D>("Backgrounds/Mountain/" + i), 0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, tempSpeed));
                tempSpeed += 0.07f;
            }
            for (int i = 0; i < 7; i++)
            {
                MenuBackground.Add(new backgrounds(Content.Load<Texture2D>("Backgrounds/Castle_Interior/" + i), 0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, tempSpeed));
                if (i < 5)
                    tempSpeed += 0.05f;

            }

            /*
             * 1 = Dirt
             * 2 = Top Left
             * 3 = Top Right
             * 4 = Top Straight
             * 5 = Bottom Straight
             * 6 = Right Straigt
             * 7 = Left Straight
             * 8 = Bottom Left
             * 9 = Bottom Right
             */

            CurrentMap = new Map();







            // 0 is the Tutorial and the others are the boss/survival waves 

            base.Initialize();
        }


        protected override void LoadContent()
        {
#if DEBUG
            debugPixel = Content.Load<Texture2D>("debug/Pixel");
            debugFont = Content.Load<SpriteFont>("debug/debugFont");
#endif

            MenuSound = Content.Load<SoundEffect>("Sound/MenuSound");

            Menu = Content.Load<SpriteFont>("Menu/MenuFont");

            textEffect = Content.Load<Effect>("Shader/Rainbow");

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            //adds all the projectiles textures and hud textures in one place for the player to use
            Projectiles_Textures.Add(Content.Load<Texture2D>("Spells/Bat"));
            Projectiles_Textures.Add(Content.Load<Texture2D>("Projectiles/arrow"));
            Projectiles_Textures.Add(Content.Load<Texture2D>("Projectiles/Knife"));
            Projectiles_Textures.Add(Content.Load<Texture2D>("Spells/Skeleton Sheet"));
            Projectiles_Textures.Add(Content.Load<Texture2D>("Projectiles/Bolt"));
            Projectiles_Textures.Add(Content.Load<Texture2D>("Hud-Elements/Heal"));
            Projectiles_Textures.Add(Content.Load<Texture2D>("Hud-Elements/Full-Counter"));
            Projectiles_Textures.Add(Content.Load<Texture2D>("Hud-Elements/Y-Button"));

            // same idea as the textures so its loaded in the player for them to use
            Sounds.Add(Content.Load<SoundEffect>("Sound/Bat wings"));
            Sounds.Add(Content.Load<SoundEffect>("Sound/ArrowImpact"));
            Sounds.Add(Content.Load<SoundEffect>("Sound/Magic Exploding"));
            Sounds.Add(Content.Load<SoundEffect>("Sound/Walking Player"));



            CurrentSound = Content.Load<Song>("Music/BrokenVillage");
            MediaPlayer.Play(CurrentSound);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.5f;
        }


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime)
        {
            NewPad = GamePad.GetState(PlayerIndex.One);
#if DEBUG
            // this is for debug purposes to exit out of the game quickly if an issue is found without having to go through all the menus and are in the debug mode
            // otherwise just alt + F4 it 
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
#endif  

            if (Change == false)
            {

                effectoffset += (float)gameTime.ElapsedGameTime.TotalSeconds / 2;

                if (effectoffset > 0.99f)
                {
                    Change = true;
                }
            }
            else
            {
                effectoffset = (effectoffset - (float)gameTime.ElapsedGameTime.TotalSeconds / 2);
                if (effectoffset < -0.99f)
                {
                    Change = false;
                }
            }
            textEffect.Parameters["threshold"].SetValue(effectoffset);




            // TODO: Add your update logic here
            switch (CurrentGameState)
            {
                #region startscreen
                case GameState.StartScreen:
                    StartMenuUpdate();

                    break;
                #endregion

                #region Controls
                case GameState.Controls:

                    foreach (backgrounds back in MenuBackground)
                    {
                        back.MenuUpdate();
                    }
                    if (NewPad.Buttons.B == ButtonState.Pressed && OldPad.Buttons.B == ButtonState.Released)
                    {
                        CurrentGameState = GameState.StartScreen;
                    }

                    break;
                #endregion

                #region Playing
                case GameState.Playing:
                    if (Player.HEALTH <= 0)
                    {
                        CurrentGameState = GameState.GameOver;
                        Player.DisableSound();
                        foreach (Enemies enemy in EnemyList)
                        {
                            enemy.STOPSOUND();
                        }
                    }
                    if (NewPad.Buttons.Start == ButtonState.Pressed && OldPad.Buttons.Start == ButtonState.Released)
                    {
                        CurrentGameState = GameState.Paused;
                        selection = 0;
                        effectoffset = 0;
                    }
                    //this does the switching of the levels that require the player to kill all the enemies first before they can progress
                    ArenaRoomWinConditions();
                    PlayingUpdate(gameTime);
                    break;
                #endregion

                #region Paused

                case GameState.Paused:

                    PauseUpdate();

                    break;

                #endregion



                #region Game Over
                case GameState.GameOver:

                    GameOverUpdate();

                    break;
                #endregion

                #region Load
                case GameState.Load:
                    LoadUpdate(gameTime);

                    break;
                    #endregion

            }

            OldPad = NewPad;
            base.Update(gameTime);
        }
        /// <summary>
        /// 
        ///             this keeps all of the load update in one place and easier to find if there is an issue in this section of code
        /// 
        /// </summary>
        private void LoadUpdate(GameTime gameTime)
        {
            //this is to stop an issue with the sound still playing when there's no enemies
            for (int i = EnemyList.Count - 1; i >= 0; i--)
            {
                EnemyList[i].STOPSOUND();
                EnemyList.RemoveAt(i);
            }
            //Player.DisableSound();
            foreach (backgrounds back in MenuBackground)
            {
                back.MenuUpdate();
            }
            // this does the sprite effect on what is selected
            if (Change == false)
            {

                effectoffset += (float)gameTime.ElapsedGameTime.TotalSeconds / 2;

                if (effectoffset > 0.99f)
                {
                    Change = true;
                }
            }
            else
            {
                effectoffset = (effectoffset - (float)gameTime.ElapsedGameTime.TotalSeconds / 2);
                if (effectoffset < -0.99f)
                {
                    Change = false;
                }
            }
            textEffect.Parameters["threshold"].SetValue(effectoffset);

            //this is for changing the selection on the screen

            if (NewPad.DPad.Down == ButtonState.Pressed && OldPad.DPad.Down == ButtonState.Released)
            {
                selection++;
                MenuSound.Play();
            }
            if (NewPad.DPad.Up == ButtonState.Pressed && OldPad.DPad.Up == ButtonState.Released)
            {
                selection--;
                MenuSound.Play();

            }
            //this will make sure it loops on itself so it cant be a value outsidce tof the menu selection
            if (selection < 0)
            {
                selection = 1;
            }
            if (selection > 1)
            {
                selection = 0;
            }



            if (NewPad.Buttons.A == ButtonState.Pressed && OldPad.Buttons.A == ButtonState.Released)
            {
                if (selection == 0) //goes to the next level
                {
                    switch (CurrentLevel)
                    {
                        case LEVEL.Level1:
                            Level1();
                            break;
                        case LEVEL.Level2:
                            Level2();
                            break;
                        case LEVEL.Level3:
                            Level3();
                            break;
                        case LEVEL.Level4:
                            Level4();
                            break;
                        case LEVEL.Level5:
                            Level5();
                            break;
                        case LEVEL.Boss1:
                            Level6();
                            break;
                    }

                    CurrentGameState = GameState.Playing;
                }
                if (selection == 1) //exits back to the start menu
                {
                    selection = 0;
                    CurrentGameState = GameState.StartScreen;
                    CurrentLevel = LEVEL.TestArea;
                }
            }
        }
        
        /// <summary>
        /// 
        ///             This keeps all of the Gameover update in one place and makes it easier to fix any issues as its in one place
        ///             
        /// </summary>
        
        private void GameOverUpdate()
        {
            if (NewPad.DPad.Down == ButtonState.Pressed && OldPad.DPad.Down == ButtonState.Released)
            {
                selection++;
                MenuSound.Play();
            }
            if (NewPad.DPad.Up == ButtonState.Pressed && OldPad.DPad.Up == ButtonState.Released)
            {
                selection--;
                MenuSound.Play();

            }
            if (selection < 0)
            {
                selection = 1;
            }
            if (selection > 1)
            {
                selection = 0;
            }
            if (NewPad.Buttons.A == ButtonState.Pressed && OldPad.Buttons.A == ButtonState.Released)
            {
                //this will be for restarting the game
                if (selection == 0)
                {
                    switch (CurrentLevel)
                    {
                        case LEVEL.TestArea:
                            Level0();
                            CurrentGameState = GameState.Playing;
                            break;
                        case LEVEL.Level1:
                            Level1();
                            CurrentGameState = GameState.Playing;
                            break;
                        case LEVEL.Level2:
                            Level2();
                            CurrentGameState = GameState.Playing;
                            break;
                        case LEVEL.Level3:
                            Level3();
                            CurrentGameState = GameState.Playing;
                            break;
                        case LEVEL.Level4:
                            Level4();
                            CurrentGameState = GameState.Playing;
                            break;
                        case LEVEL.Level5:
                            Level5();
                            CurrentGameState = GameState.Playing;
                            break;
                        case LEVEL.Boss1:
                            Level6();
                            CurrentGameState = GameState.Playing;
                            break;
                    }
                }
                //for loading the game
                if (selection == 1)
                {
                    CurrentGameState = GameState.StartScreen;
                    selection = 0;
                }
            }
        }


        /// <summary>
        /// 
        ///             This keeps all of the Pause update in one place and makes it easier to fix any issues as its in one place
        ///             
        /// </summary>

        private void PauseUpdate()
        {
            textEffect.Parameters["threshold"].SetValue(effectoffset);


            if (NewPad.DPad.Down == ButtonState.Pressed && OldPad.DPad.Down == ButtonState.Released)
            {
                selection++;
                MenuSound.Play();
            }
            if (NewPad.DPad.Up == ButtonState.Pressed && OldPad.DPad.Up == ButtonState.Released)
            {
                selection--;
                MenuSound.Play();

            }
            //this will make sure it loops on itself so it cant be a value outsidce tof the menu selection
            if (selection < 0)
            {
                selection = 2;
            }
            if (selection > 2)
            {
                selection = 0;
            }
            if (NewPad.Buttons.A == ButtonState.Pressed && OldPad.Buttons.A == ButtonState.Released)
            {
                //this will continue the game
                if (selection == 0)
                {
                    CurrentGameState = GameState.Playing;
                }
                //This will restart the game
                if (selection == 1)
                {
                    for (int i = EnemyList.Count - 1; i >= 0; i--)
                    {
                        EnemyList[i].STOPSOUND();
                        EnemyList.RemoveAt(i);
                    }
                    switch (CurrentLevel)
                    {
                        case LEVEL.TestArea:
                            Level0();
                            break;
                        case LEVEL.Level1:
                            Level1();
                            break;
                        case LEVEL.Level2:
                            Level2();
                            break;
                        case LEVEL.Level3:
                            Level3();
                            break;
                        case LEVEL.Level4:
                            Level4();
                            break;
                        case LEVEL.Level5:
                            Level5();
                            break;
                        case LEVEL.Boss1:
                            Level6();
                            break;

                    }
                    CurrentGameState = GameState.Playing;
                }
                //THis will go back to the start screen
                if (selection == 2)
                {
                    CurrentGameState = GameState.StartScreen;
                    selection = 0;
                }

            }
        }

        /// <summary>
        ///
        /// this keeps all the update for when actually "playing" the game in one place
        /// 
        /// this is so it's easier to make any changes with the update for the playing state as it will all be contained in one place and away from the cluster of code within the update section 
        /// 
        /// </summary>

        private void PlayingUpdate(GameTime gameTime)
        {


            if (CurrentLevel != LEVEL.Boss1)
            {
                #region ScreenBounds

                /*
                 * this holds all the code for the screen bounds
                 * 
                 * The reasoning for all these if statement is so that the screens collisions will stay in the right place
                 * depending on where the player is
                 * 
                 */

                //left side of the map
                if (Player.PlayerPos.X < graphics.GraphicsDevice.Viewport.Width / 2)
                {
                    if (Player.PlayerPos.Y < graphics.GraphicsDevice.Viewport.Height / 2)
                    {
                        ScreenBounds = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                    }
                    else if (Player.PlayerPos.Y > CurrentMap.Height - graphics.PreferredBackBufferHeight / 2)
                    {
                        ScreenBounds = new Rectangle(0, CurrentMap.Height - graphics.PreferredBackBufferHeight, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

                    }
                    else
                    {
                        ScreenBounds = new Rectangle(0, (int)(Player.PlayerPos.Y - graphics.PreferredBackBufferHeight / 2), graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

                    }
                }
                // right side of the map
                else if (Player.PlayerPos.X > CurrentMap.Width - graphics.GraphicsDevice.Viewport.Width / 2)
                {

                    if (Player.PlayerPos.Y < graphics.GraphicsDevice.Viewport.Height / 2)
                    {
                        ScreenBounds = new Rectangle(CurrentMap.Width - graphics.GraphicsDevice.Viewport.Width, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

                    }
                    else if (Player.PlayerPos.Y > CurrentMap.Height - graphics.PreferredBackBufferHeight / 2)
                    {
                        ScreenBounds = new Rectangle(CurrentMap.Width - graphics.GraphicsDevice.Viewport.Width, CurrentMap.Height - graphics.PreferredBackBufferHeight, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

                    }
                    else
                    {
                        ScreenBounds = new Rectangle(CurrentMap.Width - graphics.GraphicsDevice.Viewport.Width, (int)(Player.PlayerPos.Y - graphics.PreferredBackBufferHeight / 2), graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);


                    }

                }
                //in the middle of the map
                else
                {

                    if (Player.PlayerPos.Y < graphics.GraphicsDevice.Viewport.Height / 2)
                    {
                        ScreenBounds = new Rectangle((int)(Player.PlayerPos.X - graphics.PreferredBackBufferWidth / 2), 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

                    }
                    else if (Player.PlayerPos.Y > CurrentMap.Height - graphics.PreferredBackBufferHeight / 2)
                    {
                        ScreenBounds = new Rectangle((int)(Player.PlayerPos.X - graphics.PreferredBackBufferWidth / 2), CurrentMap.Height - graphics.PreferredBackBufferHeight, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

                    }
                    else
                    {
                        ScreenBounds = new Rectangle((int)(Player.PlayerPos.X - graphics.PreferredBackBufferWidth / 2), (int)(Player.PlayerPos.Y - graphics.PreferredBackBufferHeight / 2), graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

                    }

                }
                #endregion
            }
            //this will make the screen bounds the default
            else
            {
                ScreenBounds = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            }

            foreach (backgrounds Background in CurrentBack)
            {
                Background.Update(Player, CurrentLevel);
            }
            foreach (Enemies enemy in EnemyList)
            {
                if (enemy.COLLISION.Intersects(ScreenBounds))
                {
                    enemy.Update(CurrentMap.CollisionTiles, Player, ScreenBounds, gameTime, CurrentGameState);
                }
                else
                {
                    enemy.BackgroundUpdate(CurrentMap.CollisionTiles, Player, ScreenBounds, gameTime, CurrentGameState);
                }

            }
            for (int i = 0; i < EnemyList.Count; i++)
            {
                if (EnemyList[i].VisibleItIs == false)
                {
                    EnemyList.RemoveAt(i);
                }

            }
            //this will be for the health upgrades
            foreach (HealthUpgrades heart in hUpgrades)
            {
                if (Player.COLLISION.Intersects(heart.COLLISION))
                {
                    Player.MAXHEALTH += 25;
                    Player.HEALTH += 25;
                    heart.VISIBLE = false;
                }

            }
            for (int h = 0; h < hUpgrades.Count; h++)
            {
                if (hUpgrades[h].VISIBLE == false)
                {
                    hUpgrades.RemoveAt(h);

                }
            }
            if (Player.COLLISION.Intersects(ExitPoint))
            {
                if (NewPad.Buttons.B == ButtonState.Pressed && OldPad.Buttons.B == ButtonState.Released)
                {
                    Player.DisableSound();
                    switch (CurrentLevel)
                    {
                        case LEVEL.Level1:
                            CurrentLevel = LEVEL.Level2;
                            break;
                        case LEVEL.Level3:
                            CurrentLevel = LEVEL.Level4;

                            break;
                        case LEVEL.Level5:
                            CurrentLevel = LEVEL.Boss1;
                            break;
                    }
                    saveManager.Add(CurrentLevel, Player);
                    CurrentGameState = GameState.Load;
                }
            }
            Player.Update(gameTime, NewPad, OldPad, CurrentMap.CollisionTiles, EnemyList, ScreenBounds);

        }


        /// <summary>
        /// 
        ///             This is specifically for the levels where the camera won't move. this is so the conditions for continuing in these specific levels are in one place and easy to find
        ///             
        ///             The Conditions for continuing in these levels is that the player defeats all the enemies in order to continue and the prompt to press the B button will come up when they have
        ///             
        /// 
        ///             
        /// </summary>


        private void ArenaRoomWinConditions()
        {
            switch (CurrentLevel)
            {
                default:

                    break;
                case LEVEL.TestArea:

                    if (NewPad.Buttons.B == ButtonState.Pressed && OldPad.Buttons.B == ButtonState.Released && EnemyList.Count <= 0)
                    {
                        foreach (Enemies enemy in EnemyList)
                        {
                            enemy.VisibleItIs = false;
                        }
                        CurrentGameState = GameState.Load;
                        CurrentLevel = LEVEL.Level1;
                        Player.DisableSound();
                    }
                    break;
                case LEVEL.Level2:
                    if (NewPad.Buttons.B == ButtonState.Pressed && OldPad.Buttons.B == ButtonState.Released && EnemyList.Count <= 0)
                    {
                        foreach (Enemies enemy in EnemyList)
                        {
                            enemy.VisibleItIs = false;
                        }
                        CurrentGameState = GameState.Load;
                        CurrentLevel = LEVEL.Level3;
                        Player.DisableSound();

                    }


                    break;
                case LEVEL.Level4:
                    if (NewPad.Buttons.B == ButtonState.Pressed && OldPad.Buttons.B == ButtonState.Released && EnemyList.Count <= 0)
                    {
                        foreach (Enemies enemy in EnemyList)
                        {
                            enemy.VisibleItIs = false;
                        }
                        CurrentGameState = GameState.Load;
                        CurrentLevel = LEVEL.Level5;
                        Player.DisableSound();

                    }


                    break;
                case LEVEL.Boss1:
                    if (NewPad.Buttons.B == ButtonState.Pressed && OldPad.Buttons.B == ButtonState.Released && EnemyList.Count <= 0)
                    {
                        foreach (Enemies enemy in EnemyList)
                        {
                            enemy.VisibleItIs = false;
                        }
                        CurrentGameState = GameState.StartScreen;
                        Player.DisableSound();

                    }
                    break;




            }
        }

        /// <summary>
        /// 
        ///             this is to keep the start menu code for updating in one place and is easier to change if anything needs to be changed
        /// 
        /// </summary>
        private void StartMenuUpdate()
        {
            foreach (backgrounds back in MenuBackground)
            {
                back.MenuUpdate();
            }
            // this does the sprite effect on what is selected


            //this is for changing the selection on the screen and plays a sound for the menu

            if (NewPad.DPad.Down == ButtonState.Pressed && OldPad.DPad.Down == ButtonState.Released)
            {
                selection++;
                MenuSound.Play();
            }
            if (NewPad.DPad.Up == ButtonState.Pressed && OldPad.DPad.Up == ButtonState.Released)
            {
                selection--;
                MenuSound.Play();

            }
            //this will make sure it loops on itself so it cant be a value outside of the menu selection
            if (selection < 0)
            {
                selection = 3;
            }
            if (selection > 3)
            {
                selection = 0;
            }
            //this will do whatever it needs to when the player hits the A button such as start the game or close it
            if (NewPad.Buttons.A == ButtonState.Pressed && OldPad.Buttons.A == ButtonState.Released)
            {
                //this will be for the new game
                if (selection == 0)
                {
                    NewGame();
                    Level0();
                    CurrentGameState = GameState.Playing;
                    CurrentLevel = LEVEL.TestArea;
                }
                //for loading the game
                if (selection == 1)
                {
                    Continue();
                }
                //controls
                if (selection == 2)
                {
                    CurrentGameState = GameState.Controls;
                }
                //exit
                if (selection == 3)
                {
                    Exit();
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.CornflowerBlue);
            switch (CurrentGameState)
            {
                #region startScreen
                case GameState.StartScreen:

                    StartScreenDraw(gameTime);

                    break;
                #endregion

                #region Controls
                case GameState.Controls:


                    spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null, null, null);



                    foreach (backgrounds background in MenuBackground)
                    {
                        background.Draw(spriteBatch, gameTime);
                    }

                    spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle(0, 0, ScreenBounds.Width, ScreenBounds.Height), Color.Black * 0.45f);


                    spriteBatch.Draw(Content.Load<Texture2D>("Menu/cont"), new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Color.White);

                    spriteBatch.End();

                    break;
                #endregion

                #region Playing
                case GameState.Playing:
                    PlayingDraw(gameTime);
#if DEBUG
                    spriteBatch.Begin();

                    spriteBatch.DrawString(debugFont, "\nfps: " + (int)(1 / gameTime.ElapsedGameTime.TotalSeconds), new Vector2(200, 500), Color.Red);

                    spriteBatch.End();
#endif
                    break;
                #endregion

                #region Paused
                case GameState.Paused:


                    PlayingDraw(gameTime);
                    PauseDraw();

                    break;
                #endregion

                #region Load
                case GameState.Load:
                    LoadDraw(gameTime);
                    break;
                #endregion

                #region GameOver
                case GameState.GameOver:


                    PlayingDraw(gameTime);

                    GameOverDraw();
                    break;
                    #endregion
            }
        }

        /// <summary>
        ///     
        ///             This keeps all the Gameovers drawing in one place and makes it simpler to change if it needs to be changed
        ///             
        ///             this will draw pretty much identically to the start screen which has been listed in the summary for that with a change to what the items in the selections and the background will be completly blacked out
        /// 
        /// </summary>

        private void GameOverDraw()
        {
            spriteBatch.Begin();
            //these are the boxes for the selections in the menu
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) - 20, 295, (int)Menu.MeasureString("New Game").X + 40, 200), Color.DarkBlue * 0.5f);

            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Retry").X / 2) - 10, 300, (int)Menu.MeasureString("Retry").X + 20, (int)Menu.MeasureString("Retry").Y + 5), Color.Black * 0.45f);
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Exit").X / 2) - 10, 350, (int)Menu.MeasureString("Exit").X + 20, (int)Menu.MeasureString("Exit").Y + 5), Color.Black * 0.45f);

            if (selection == 0)
            {
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 350), Color.White);
            }
            if (selection == 1)
            {
                spriteBatch.DrawString(Menu, "Retry", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Retry").X / 2, 300), Color.White);
            }
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, textEffect);
            spriteBatch.DrawString(Menu, "Game Over", new Vector2(graphics.PreferredBackBufferWidth / 2 - Menu.MeasureString("Game Over").X / 2,200), Color.White);

            if (selection == 0)
            {
                spriteBatch.DrawString(Menu, "Retry", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Retry").X / 2, 300), Color.White);
            }
            if (selection == 1)
            {
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 350), Color.White);
            }


            spriteBatch.End();
        }

        /// <summary>
        ///  
        ///         this keeps all of the loading screen drawing in one place and makes it simpler to change it if it needs to be 
        ///         
        ///         this will draw pretty much identically to the start screen which has been listed in the summary for that with a change to what the items in the selections
        ///  
        /// </summary>

        private void LoadDraw(GameTime gameTime)
        {
            // a lot of the draw for this screen is the same as the start screen


            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null, null, null);

            foreach (backgrounds back in MenuBackground)
            {
                back.Draw(spriteBatch, gameTime);
            }
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle(0, 0, ScreenBounds.Width, ScreenBounds.Height), Color.Black * 0.45f);

            //this is the backing of the of the text on the load screen

            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) - 20, 295, (int)Menu.MeasureString("New Game").X + 40, 200), Color.DarkBlue * 0.5f);

            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Continue").X / 2) - 10, 300, (int)Menu.MeasureString("Continue").X + 20, (int)Menu.MeasureString("Continue").Y + 5), Color.Black * 0.45f);
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Exit").X / 2) - 10, 350, (int)Menu.MeasureString("Exit").X + 20, (int)Menu.MeasureString("Exit").Y + 5), Color.Black * 0.45f);

            //this is the text getting drawn
            if (selection == 0)
            {
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 350), Color.White);
            }
            if (selection == 1)
            {
                spriteBatch.DrawString(Menu, "Continue", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Continue").X / 2, 300), Color.White);
            }
            spriteBatch.End();

            //this draws the text to show the text clearly selected
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, textEffect);
            if (selection == 0)
            {
                spriteBatch.DrawString(Menu, "Continue", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Continue").X / 2, 300), Color.White);
            }
            if (selection == 1)
            {
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 350), Color.White);
            }


            spriteBatch.End();
        }

        /// <summary>
        /// 
        /// this keeps all of the pause screen drawing in one place so that it's easier to change if required
        /// 
        /// this will draw pretty much identically to the start screen which has been listed in the summary for that with a change to what the items in the selections
        /// 
        /// </summary>

        private void PauseDraw()
        {
            spriteBatch.Begin();
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle(0, 0, ScreenBounds.Width, ScreenBounds.Height), Color.Black * 0.45f);

            //this is a backing thats underneath the backing of the text
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) - 20, 295, (int)Menu.MeasureString("New Game").X + 40, 200), Color.DarkBlue * 0.5f);


            //this draws a backing underneath the text
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Resume").X / 2) - 10, 300, (int)Menu.MeasureString("Resume").X + 20, (int)Menu.MeasureString("Resume").Y + 5), Color.Black * 0.45f);
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Restart").X / 2) - 10, 350, (int)Menu.MeasureString("Restart").X + 20, (int)Menu.MeasureString("Restart").Y + 5), Color.Black * 0.45f);
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Exit").X / 2) - 10, 400, (int)Menu.MeasureString("Exit").X + 20, (int)Menu.MeasureString("Controls").Y + 5), Color.Black * 0.45f);


            if (selection == 0)
            {
                spriteBatch.DrawString(Menu, "Restart", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Restart").X / 2, 350), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 400), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            }
            if (selection == 1)
            {
                spriteBatch.DrawString(Menu, "Resume", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Resume").X / 2, 300), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 400), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            if (selection == 2)
            {
                spriteBatch.DrawString(Menu, "Resume", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Resume").X / 2, 300), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Restart", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Restart").X / 2, 350), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }

            spriteBatch.End();

            //this applies a visual to the text to show what one is selected
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, textEffect);

            if (selection == 0)
            {
                spriteBatch.DrawString(Menu, "Resume", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Resume").X / 2, 300), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            if (selection == 1)
            {
                spriteBatch.DrawString(Menu, "Restart", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Restart").X / 2, 350), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            if (selection == 2)
            {
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 400), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }

            spriteBatch.End();
        }

        /// <summary>
        /// 
        ///         This is used for drawing the start menu onto the screen when the user is on this screen
        ///         
        ///         this makes it easier to make changes to the code if anything needs to be altered 
        ///         
        ///         This will draw the current Background, an overlay for the background to darken it, transparent blue menu box with the selections within this and have a black transparency between the text and the menu box
        ///         
        ///         the one that is selected will use a shader to show what one is clearly seleceted
        /// 
        /// </summary>
 

        private void StartScreenDraw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null, null, null);

            foreach (backgrounds back in MenuBackground)
            {
                back.Draw(spriteBatch, gameTime);
            }
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle(0, 0, ScreenBounds.Width, ScreenBounds.Height), Color.Black * 0.45f); //creates a slightly black overlay over the background


            //these are the boxes for the selections in the menu
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) - 20, 295, (int)Menu.MeasureString("New Game").X + 40, 200), Color.DarkBlue * 0.5f);


            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) - 10, 300, (int)Menu.MeasureString("New Game").X + 20, (int)Menu.MeasureString("New Game").Y + 5), Color.Black * 0.45f);
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Continue").X / 2) - 10, 350, (int)Menu.MeasureString("Continue").X + 20, (int)Menu.MeasureString("Continue").Y + 5), Color.Black * 0.45f);
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Controls").X / 2) - 10, 400, (int)Menu.MeasureString("Controls").X + 20, (int)Menu.MeasureString("Controls").Y + 5), Color.Black * 0.45f);
            spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("Exit").X / 2) - 10, 450, (int)Menu.MeasureString("Exit").X + 20, (int)Menu.MeasureString("Exit").Y + 5), Color.Black * 0.45f);

            //thios will show what buttons have to be pressed to control the menu

            spriteBatch.Draw(Content.Load<Texture2D>("Menu/A"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) - 20, 525, 50, 50), Color.White);
            spriteBatch.Draw(Content.Load<Texture2D>("Menu/dPad"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) - 20, 590, 40, 50), Color.White);
            spriteBatch.Draw(Content.Load<Texture2D>("Menu/dPad"), new Rectangle((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) + 30, 590, 40, 50), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 1f);

            spriteBatch.DrawString(Menu, "Select", new Vector2((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) + 35, 530), Color.White);
            spriteBatch.DrawString(Menu, "Change \nSelection", new Vector2((ScreenBounds.Width / 2 - (int)Menu.MeasureString("New Game").X / 2) + 75, 585), Color.White);


            //the -menu.measurestring("whatever").x/2 is to center it to the screen 
            if (selection == 0)
            {
                spriteBatch.DrawString(Menu, "Continue", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Continue").X / 2, 350), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Controls", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Controls").X / 2, 400), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 450), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);

            }
            if (selection == 1)
            {
                spriteBatch.DrawString(Menu, "New Game", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("New Game").X / 2, 300), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Controls", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Controls").X / 2, 400), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 450), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            if (selection == 2)
            {
                spriteBatch.DrawString(Menu, "New Game", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("New Game").X / 2, 300), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Continue", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Continue").X / 2, 350), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 450), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            if (selection == 3)
            {
                spriteBatch.DrawString(Menu, "New Game", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("New Game").X / 2, 300), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Continue", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Continue").X / 2, 350), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
                spriteBatch.DrawString(Menu, "Controls", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Controls").X / 2, 400), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }

            spriteBatch.End();

            //this applys the shader onto the one that is selected
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, textEffect);

            textEffect.CurrentTechnique.Passes[0].Apply();

            spriteBatch.DrawString(Menu, "Twisted Soul", new Vector2(graphics.PreferredBackBufferWidth / 2 - Menu.MeasureString("Twisted Soul").X, 150), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1f);

            if (selection == 0)
            {
                spriteBatch.DrawString(Menu, "New Game", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("New Game").X / 2, 300), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            if (selection == 1)
            {
                spriteBatch.DrawString(Menu, "Continue", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Continue").X / 2, 350), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            if (selection == 2)
            {
                spriteBatch.DrawString(Menu, "Controls", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Controls").X / 2, 400), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }
            if (selection == 3)
            {
                spriteBatch.DrawString(Menu, "Exit", new Vector2(ScreenBounds.Width / 2 - Menu.MeasureString("Exit").X / 2, 450), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
            }


            spriteBatch.End();
        }

        /// <summary>
        /// 
        /// this keeps all the drawing that happens whilst playing in one place.
        /// 
        /// this basically makes it easier to call another instance of it if it is need in a diffrent state such as the pause menu
        /// 
        /// </summary>

        private void PlayingDraw(GameTime gameTime)
        {
            // not exclusivly for the background drawing but in general thats what this one is used for when the camera moves 
            #region Background Draw

            // allows the background to loop on itself using the source rectangle
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearWrap, null, null, null, null);

            foreach (backgrounds background in CurrentBack)
            {
                background.Draw(spriteBatch, gameTime);
            }

            switch (CurrentLevel)
            {
                //this is for any of the levels that don't have the camera move for them
                default:
                    foreach (Enemies enemy in EnemyList)
                    {
                        // draws the enemy that are on screen otherwise it will draw their projectiles
                        if (enemy.COLLISION.Intersects(ScreenBounds))
                            enemy.Draw(spriteBatch, gameTime);
                        else
                            enemy.OffscreenDraw(spriteBatch, gameTime);
                    }

                    CurrentMap.Draw(spriteBatch, ScreenBounds);
                    if (CurrentGameState == GameState.GameOver)
                    {
                        spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), new Rectangle(0,0,ScreenBounds.Width,ScreenBounds.Height), Color.Black);
                    }
                    Player.Draw(spriteBatch, gameTime);

                    if (CurrentGameState != GameState.GameOver)
                        if(EnemyList.Count <= 0) 
                        {
                            spriteBatch.DrawString(Menu, "Press B to continue", new Vector2(graphics.PreferredBackBufferWidth / 2 - Menu.MeasureString("Press B to continue").X / 2, graphics.PreferredBackBufferHeight / 4), Color.Red);
                        }

                    break;


                case LEVEL.Level1:


                    break;
                case LEVEL.Level3:


                    break;
                case LEVEL.Level5:


                    break;
            }

            spriteBatch.End();

            #endregion

            #region Camera Spritebatch

            /*
             *This is for drawing everything when the game is getting played
             * this is due to the camera following the player when it is getting played on certain levels where the camera won't use a fixed posion
             */
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Matrix.CreateTranslation(new Vector3(Player.CameraMovement(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, CurrentLevel, CurrentMap).X, Player.CameraMovement(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, CurrentLevel, CurrentMap).Y, 0)));
            switch (CurrentLevel)
            {
                default:
                    foreach (Enemies enemy in EnemyList)
                    {
                        if(enemy.COLLISION.Intersects(ScreenBounds))
                        enemy.Draw(spriteBatch, gameTime);
                    }
                    CurrentMap.Draw(spriteBatch, ScreenBounds);
                    if (CurrentGameState == GameState.GameOver)
                    {
                        spriteBatch.Draw(Content.Load<Texture2D>("debug/Pixel"), ScreenBounds, Color.Black);
                    }
                    foreach (HealthUpgrades heart in hUpgrades)
                    {
                        heart.Draw(spriteBatch, gameTime);
                    }
                    spriteBatch.Draw(Content.Load<Texture2D>("Debug/Pixel"), ExitPoint, Color.Black* 0.25f);
                    Player.Draw(spriteBatch, gameTime);
                    break;
                case LEVEL.Boss1:

                    foreach (Boss boss in EnemyList)
                    {
                        boss.DrawHealth(spriteBatch, graphics, Content.Load<Texture2D>("Hud-Elements/EnemyHealth"),
                                                                 Content.Load<Texture2D>("Hud-Elements/OuterBar"),
                                                                 Content.Load<Texture2D>("Hud-Elements/BarEndCap"),
                                                                 HudFont);
                    }

                    break;
                case LEVEL.TestArea:

                    break;
                case LEVEL.Level2:

                    break;
                case LEVEL.Level4:

                    break;



            }


#if DEBUG
            spriteBatch.Draw(debugPixel, ScreenBounds, Color.Chartreuse * 0.5f);
            
#endif

            spriteBatch.End();

            #endregion

            #region exit Text
            //this is only for the exit text to show where the player is actually going but applies a sprite effect to the text
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, textEffect, Matrix.CreateTranslation(new Vector3(Player.CameraMovement(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, CurrentLevel, CurrentMap).X, Player.CameraMovement(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, CurrentLevel, CurrentMap).Y, 0)));

            if (CurrentLevel == LEVEL.Level1 || CurrentLevel == LEVEL.Level3 || CurrentLevel == LEVEL.Level5)
                spriteBatch.DrawString(Menu, "Next Level", new Vector2(ExitPoint.X + ExitPoint.Width / 2, ExitPoint.Y), Color.White);

            spriteBatch.End();

            #endregion

            #region Stationary Elements

            /*
             * This is for drawing things that won't move on the screen
             * This will be things like the players HUD elements and the menus
             */

            spriteBatch.Begin();

            Player.DrawHud(spriteBatch, Content.Load<Texture2D>("Hud-Elements/Health"),
                Content.Load<Texture2D>("Hud-Elements/Mana"),
                Content.Load<Texture2D>("Hud-Elements/OuterBar"),
                Content.Load<Texture2D>("Hud-Elements/BarEndCap"),
                HudFont);
            if(Player.COLLISION.Intersects(ExitPoint))
            spriteBatch.DrawString(Menu, "Press B to exit", new Vector2(ScreenBounds.Width/2,ScreenBounds.Height/2),Color.Red) ;

            spriteBatch.End();
            #endregion
        }
        /// <summary>
        /// 
        /// this sets the player health and mana whenever they select continue,restart or any other instance when ever they load a previous save state
        /// 
        /// </summary>
        private void LoadPlayer()
        {
            Player.HEALTH = saveManager.DATA.Health + 100;
            Player.MANA = saveManager.DATA.MaxMana;
            Player.MAXMANA = saveManager.DATA.MaxMana;
            Player.MAXHEALTH = saveManager.DATA.MaxHealth;

            if(CurrentLevel == LEVEL.Boss1) // this is more to give the player a bit of a chance in case they finished the previous level with 1 hp
            {
                Player.HEALTH = saveManager.DATA.MaxHealth;
            }
        }
        /// <summary>
        /// this will reset the values in the save for the game when the player hits new game
        /// </summary>
        private void NewGame()
        {
            //this resets the Current Value of the Player
            saveManager.RESET();
            CurrentGameState = GameState.Playing;
        }
        /// <summary>
        ///     this will load player into the correct level by calling the method related to the current level
        /// </summary>
        private void Continue()
        {
            CurrentLevel = (LEVEL)saveManager.DATA.Level;
            switch (CurrentLevel)
            {
                case LEVEL.TestArea:
                    Level0();
                    break;

                case LEVEL.Level1:

                    Level1();

                    break;
                case LEVEL.Level2:

                    Level2();

                    break;
                case LEVEL.Level3:

                    Level3();

                    break;
                case LEVEL.Level4:

                    Level4();

                    break;
                case LEVEL.Level5:

                    Level5();

                    break;
                case LEVEL.Boss1:
                    Level6();

                    break;


            }
            CurrentGameState = GameState.Playing;// this makes sure the game actually continues playing after loading the level
        }

        /// <summary>
        /// 
        /// any of the private Level+whatever value is basically how the levels are loaded into the game
        /// 
        /// this is so all the maps aren't loaded in all at once and allows for basically one map variable for storing it all as it will just read in the appropriate list to load the level
        /// 
        /// </summary>
        /// 

        #region LEVELS PRIVATE VOID
        private void Level0()
        {
            CurrentMap = new Map();
            CurrentMap.ArenaGenerate(BossTiles, Size);


            CurrentBack = DayForest;

            // add all the enemies
            EnemyList.Clear();

            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), CurrentMap.Width - 160, CurrentMap.Height - 200, 1f, 4, 20, 100, false, Content.Load<SoundEffect>("Sound/Armoured footstep")));


            //Change the song and Play

            CurrentSound = Content.Load<Song>("Music/RelaxingGreenNature");
            MediaPlayer.Play(CurrentSound);

            //add the Player
            Player = new Player(Content.Load<Texture2D>("Player/Spirit"), Content.Load<Texture2D>("Player/Hit"), 100, CurrentMap.Height - 200, 2f, 4, 10, 400, 250, Projectiles_Textures, false, Sounds);

        }
        private void Level1()
        {
            CurrentMap = new Map();

            CurrentMap.Generate(60, 32, 70, "l1.lst");

            CurrentBack = DayForest;

            ExitPoint = new Rectangle(3360, 1330, 630, 420);

            //this adds the health upgrades in the level
            hUpgrades.Clear();
            hUpgrades.Add(new HealthUpgrades(Content.Load<Texture2D>("Player/Heart"), 3570, 210));

            // add all the enemies and clear the previous list
            EnemyList.Clear();

            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 700, 1050, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 2310, 840, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 2100, 2030, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 2380, 2030, 1f, 4, 20, RNG.Next(75, 151), true, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 560, 1470, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 910, 420, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 2660, 1120, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 3500, 420, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));


            //Change the song and Play

            CurrentSound = Content.Load<Song>("Music/RelaxingGreenNature");
            MediaPlayer.Play(CurrentSound);

            //add the Player
            Player = new Player(Content.Load<Texture2D>("Player/Spirit"), Content.Load<Texture2D>("Player/Hit"), 100, CurrentMap.Height - 220, 2f, 4, 10, 400, 250, Projectiles_Textures, false, Sounds);


        }
        private void Level2()
        {
            CurrentMap = new Map();

            CurrentMap.ArenaGenerate(BossTiles, Size);
            CurrentBack = DayForest;

            hUpgrades.Clear();


            // add all the enemies
            EnemyList.Clear();

            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 100, CurrentMap.Height - 200, 1f, 4, 20, 100, true, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 300, CurrentMap.Height - 200, 1f, 4, 20, 100, true, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), CurrentMap.Width - 160, CurrentMap.Height - 200, 1f, 4, 20, 100, false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), CurrentMap.Width - 360, CurrentMap.Height - 200, 1f, 4, 20, 100, false, Content.Load<SoundEffect>("Sound/Armoured footstep")));


            //Change the song and Play

            CurrentSound = Content.Load<Song>("Music/RelaxingGreenNature");
            MediaPlayer.Play(CurrentSound);

            //add the Player

            //load the players actual stats
            Player = new Player(Content.Load<Texture2D>("Player/Spirit"), Content.Load<Texture2D>("Player/Hit"), CurrentMap.Width/2 -40, CurrentMap.Height - 220, 2f, 4, 10, 400, 250, Projectiles_Textures, false, Sounds);

            LoadPlayer();
        }
        private void Level3()
        {
            CurrentMap = new Map();
            hUpgrades = new List<HealthUpgrades>();
            CurrentMap.Generate(128, 64, 64, "l3.lst");
            CurrentBack = ForestScenes;

            ExitPoint = new Rectangle(7488, 3136, 576, 320);

            //adds the health upgrades
            hUpgrades.Clear();

            hUpgrades.Add(new HealthUpgrades(Content.Load<Texture2D>("Player/Heart"), 128, 128));
            hUpgrades.Add(new HealthUpgrades(Content.Load<Texture2D>("Player/Heart"), 896, 3072));
            hUpgrades.Add(new HealthUpgrades(Content.Load<Texture2D>("Player/Heart"), 5248, 2176));


            // add all the enemies
            EnemyList.Clear();


            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 896, 3648, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 1728, 576, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 1628, 576, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 1152, 576, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 2496, 3712, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 2176, 3712, 1f, 4, 20, RNG.Next(75, 151), true, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 3200, 3712, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 3392, 3712, 1f, 4, 20, RNG.Next(75, 151), true, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 3904, 3712, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 5312, 640, 1f, 4, 20, RNG.Next(75, 151), true, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 5376, 640, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));

            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 5367, 2176, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));


            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 384, 320, 1f, 8, 20, RNG.Next(20, 101), true, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 876, 3072, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 1728, 1984, 1f, 8, 20, RNG.Next(20, 101), true, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 1728, 2240, 1f, 8, 20, RNG.Next(20, 101), true, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 1728, 2496, 1f, 8, 20, RNG.Next(20, 101), true, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 5696, 1152, 1f, 8, 20, RNG.Next(20, 101), true, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 6400, 2240, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));


            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 384, 640, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 768, 1664, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 2880, 2624, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 3264, 2752, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 4224, 3136, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 4480, 3200, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));


            //Change the song and Play

            CurrentSound = Content.Load<Song>("Music/Elven Forest");
            MediaPlayer.Play(CurrentSound);


            //load the players actual stats
            Player = new Player(Content.Load<Texture2D>("Player/Spirit"), Content.Load<Texture2D>("Player/Hit"), 192, 2176, 2f, 4, 10, 400, 250, Projectiles_Textures, false, Sounds);

            LoadPlayer();
        }
        private void Level4()
        {
            CurrentMap = new Map();

            CurrentMap.ArenaGenerate(BossTiles, Size);
            CurrentBack = ForestScenes;

            // add all the enemies
            EnemyList.Clear();

            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 500, CurrentMap.Height - 200, 1f, 4, 20, 100, true, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 300, CurrentMap.Height - 200, 1f, 4, 20, 100, true, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), CurrentMap.Width - 560, CurrentMap.Height - 200, 1f, 4, 20, 100, false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), CurrentMap.Width - 360, CurrentMap.Height - 200, 1f, 4, 20, 100, false, Content.Load<SoundEffect>("Sound/Armoured footstep")));

            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 100, CurrentMap.Height - 200, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), CurrentMap.Width - 160, CurrentMap.Height - 200, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));

            //Change the song and Play

            CurrentSound = Content.Load<Song>("Music/Elven Forest");
            MediaPlayer.Play(CurrentSound);

            //add the Player

            //load the players actual stats
            Player = new Player(Content.Load<Texture2D>("Player/Spirit"), Content.Load<Texture2D>("Player/Hit"), CurrentMap.Width / 2 - 40, CurrentMap.Height - 220, 2f, 4, 10, 400, 250, Projectiles_Textures, false, Sounds);

            LoadPlayer();
        }
        private void Level5()
        {
            CurrentMap = new Map();

            CurrentMap.Generate(160, 32, 64, "l5.lst");//temp
            CurrentBack = CastleInterior;

            ExitPoint = new Rectangle(9472, 1280, 704, 320);

            hUpgrades.Clear();
            hUpgrades.Add(new HealthUpgrades(Content.Load<Texture2D>("Player/Heart"), 6804, 576));

            // add all the enemies to the list
            EnemyList.Clear();

            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 960, 1728, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 2368, 1664, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 2688, 1664, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 4554, 1664, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 4800, 1664, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 5248, 1664, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 6400, 768, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 6720, 768, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 7360, 1664, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 7680, 1664, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));
            EnemyList.Add(new MeleeEnemy(Content.Load<Texture2D>("Enemies/Knight/KnightComp"), Content.Load<Texture2D>("Enemies/Knight/KnightDeath"), 8000, 1664, 1f, 4, 20, RNG.Next(75, 151), false, Content.Load<SoundEffect>("Sound/Armoured footstep")));

            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 3264, 1536, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 3456, 1536, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 5248, 1664, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 6912, 1408, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 7360, 960, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 8000, 960, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 8576, 640, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 8192, 640, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 8192, 384, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));
            EnemyList.Add(new RangeEnemy(Content.Load<Texture2D>("Enemies/Ranger/Ranger"), 7680, 384, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Projectiles/arrow")));

            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 1408, 1408, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 6784, 576, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));
            EnemyList.Add(new MageEnemy(Content.Load<Texture2D>("Enemies/Mage/mage"), 7040, 1408, 1f, 8, 20, RNG.Next(20, 101), false, Content.Load<SoundEffect>("Sound/Magic Exploding"), Content.Load<Texture2D>("Projectiles/Bolt")));

            //Change the song and Play

            CurrentSound = Content.Load<Song>("Music/Dark Castle");
            MediaPlayer.Play(CurrentSound);

            //add the Player and load the correct stats

            Player = new Player(Content.Load<Texture2D>("Player/Spirit"), Content.Load<Texture2D>("Player/Hit"), 100, CurrentMap.Height - 356, 2f, 4, 10, 400, 250, Projectiles_Textures, false, Sounds);


            LoadPlayer();
        }
        private void Level6()
        {
            // loads the map and backgrounds
            CurrentMap = new Map();

            CurrentMap.BossGenerate(BossTiles, Size);

            CurrentBack = CastleInterior;

            // add all the enemies
            EnemyList.Clear();



            EnemyList.Add(new Boss(Content.Load<Texture2D>("Enemies/Boss1/Boss SpriteSheet"), CurrentMap.Width - 160, CurrentMap.Height - 200, 1f, 6, 20, 800, true, Content.Load<SoundEffect>("Sound/ArrowImpact"), Content.Load<Texture2D>("Enemies/Boss1/Sword"), Content.Load<Texture2D>("Enemies/Boss1/Sword Thrown")));

            //this is just to add the boss in and have a refrence of the original details 
            //EnemyList.Add(new Boss(Content.Load<Texture2D>("Enemies/Boss1/Boss SpriteSheet"), 600, 800, 1f, 6, 20, 100, true, Content.Load<SoundEffect>("Sound/Armoured footstep"), Content.Load<Texture2D>("Enemies/Boss1/Sword"), Content.Load<Texture2D>("Enemies/Boss1/Sword Thrown")));

            //Change the song and Play

            CurrentSound = Content.Load<Song>("Music/Dark Castle");
            MediaPlayer.Play(CurrentSound);

            //add the Player and load the correct stats
            Player = new Player(Content.Load<Texture2D>("Player/Spirit"), Content.Load<Texture2D>("Player/Hit"), 100, CurrentMap.Height - 356, 2f, 4, 10, 400, 250, Projectiles_Textures, false, Sounds);

            LoadPlayer();
        }
        #endregion


    }
}
