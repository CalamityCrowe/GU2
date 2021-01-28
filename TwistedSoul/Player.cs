using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;

namespace TwistedSoul
{
    enum PlayerStates
    {
        Attack,
        Casting,
        Dead,
        Hit,
        Idle,
        Moving,
        Throwing,
    }
    enum Spell
    {
        ConjureBats,
        FullCounter,
        Heal,
        SkeletalGrasp,
        Knife
    }
    class Player : AnimatedGraphics
    {
        PlayerStates CurrentState;
        Spell CurrentSpell;
        private Texture2D HitTexture;
        private bool second;
        private float flippedTimer;

        /// <summary>
        /// this is what all the values in the player ints represents
        /// 0 = Health, 
        /// 1 = MaxHealth, 
        /// 2 = Mana, 
        /// 3 = MaxMana, 
        /// 4 = Spell_Cost,
        /// 5 = Spell Counter,
        /// 6 = spellDamage
        /// </summary>
        private int[] Playerints = new int[7];
        public int HEALTH
        {
            get
            {
                return Playerints[0];
            }
            set
            {
                Playerints[0] = value;
            }
        }
        public int MAXHEALTH
        {
            get
            {
                return Playerints[1];
            }
            set
            {
                Playerints[1] = value;
            }

        }
        public int MAXMANA
        {
            get
            {
                return Playerints[3];
            }
            set
            {
                Playerints[3] = value;
            }
        }
        public int MANA
        {
            get
            {
                return Playerints[2];
            }
            set
            {
                Playerints[2] = value;
            }
        }

        /* Player Conditions listed 
         *  0 = deal damage
         *  1 = Blood Magic Enabled
         *  2 = has the player been hit
         *  3 = Player Has Jumped
         */
        private bool[] PlayerConditions = new bool[4];
        private readonly List<SoundEffect> ProjectileSounds;
        private bool FlipHori, FlipVert;
        private float timer = 0;


        //this is specifically used for the backgrounds so they dont move when the camera isn't moving
        public Vector2 PlayerPos
        {
            get
            {
                return Pos;
            }
        }
        public Vector2 BackgroundVelocity // this is used to apply the correct velocity for the background to move when it can
        {
            get
            {
                return Velocity;
            }
        }

        //this will be used for when the player attaches themselves to the bottom of a platform
        private SpriteEffects DoubleFlip = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;

        private List<Projectiles> PlayerProjectiles;
        public List<Projectiles> PLAYERPROJECTILES
        {
            get
            {
                return PlayerProjectiles;
            }
        }
        //this is for when they hit the enemy
        public bool DealDamage
        {
            get
            {
                return PlayerConditions[0];
            }
        }
        //this is for the enemy colliding with it the damagebox
        public Rectangle HIT
        {
            get
            {
                return DamageBox;
            }
        }

        private Rectangle CounterRect;
        //this is for when a projectile hits the counterrect
        public Rectangle COUNTERRECT
        {
            get
            {
                return CounterRect;
            }
        }

        private List<Particle> particles;

        private List<Texture2D> ProjectileText;

        public bool HasBeenHit
        {
            get
            {
                return PlayerConditions[2];
            }
        }

        // this is used in the enemy classes to damage the player
        public PlayerStates STATE
        {
            get
            {
                return CurrentState;
            }
            set
            {
                CurrentState = value;
            }
        }

        private SoundEffectInstance WalkingLoop;
        public Player(Texture2D txr, Texture2D Hit, int X, int Y, float S, int frames, int D, int H, int M, List<Texture2D> ProjectileList, bool IGNORE, List<SoundEffect> Sounds) : base(txr, X, Y, S, frames, D, IGNORE)
        {
            CurrentState = PlayerStates.Idle;
            CurrentSpell = Spell.ConjureBats;

            //this is to set the animation and collisions box to be the correct bounds
            sourceRectangle.Width = texture.Width / 4;
            sourceRectangle.Height = texture.Height / 5;
            rect.Width = texture.Width / 4;
            rect.Height = texture.Height / 5;

            //this sets the players current stats;
            Playerints[1] = H;
            Playerints[0] = Playerints[1];
            Playerints[3] = M;
            Playerints[2] = Playerints[3];

            particles = new List<Particle>();
            PlayerProjectiles = new List<Projectiles>();
            ProjectileSounds = Sounds;
            ProjectileText = ProjectileList;
            HitTexture = Hit;
            flippedTimer = 0;
#if DEBUG
            Playerints[0] = H / 2;
#endif

            // this is a tweak to the damage output for the melee attack
            Damage = D * 2;

            /*
             *  0 = deal damage
             *  1 = Blood Magic Enabled
             *  2 = has the player been hit
             *  3 = Player Has Jumped
             */

            PlayerConditions[0] = false;
            PlayerConditions[1] = false;
            PlayerConditions[2] = false;
            PlayerConditions[3] = false;

            WalkingLoop = Sounds[3].CreateInstance();
            WalkingLoop.IsLooped = true;
            WalkingLoop.Pitch = -0.5f;

        }

        public void DisableSound()
        {
            WalkingLoop.Stop();
        }


        public void Update(GameTime gt, GamePadState newPad, GamePadState oldPad, List<CollisionTiles> Tiles, List<Enemies> EnemyList, Rectangle Screen)
        {
            if (CurrentState != PlayerStates.Hit)
            {
                PlayerConditions[2] = false;
            }
            if (Playerints[2] < 0)
            {
                Playerints[2] = 0;
            }

            //this will play the sound of the player walking

            if (CurrentState == PlayerStates.Moving && Velocity.Y == 0 && Velocity.X != 0)
            {
                WalkingLoop.Play();
            }
            else
            {
                WalkingLoop.Stop();
            }

            // this assigns the cost of the spell depending what one is selcted
            switch (CurrentSpell)
            {

                #region Bats
                case Spell.ConjureBats:
                    if (newPad.DPad.Right == ButtonState.Pressed && oldPad.DPad.Right == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.FullCounter;
                    }
                    if (newPad.DPad.Left == ButtonState.Pressed && oldPad.DPad.Left == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.Knife;
                    }
                    if (PlayerConditions[1])
                    {
                        Playerints[4] = 25;
                    }
                    else
                    {
                        Playerints[4] = 15;
                    }
                    break;
                #endregion
                #region Full Counter
                case Spell.FullCounter:
                    if (newPad.DPad.Right == ButtonState.Pressed && oldPad.DPad.Right == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.Heal;
                    }
                    if (newPad.DPad.Left == ButtonState.Pressed && oldPad.DPad.Left == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.ConjureBats;
                    }
                    if (PlayerConditions[1])
                    {
                        Playerints[4] = 35;
                    }
                    else
                    {
                        Playerints[4] = 15;
                    }
                    break;
                #endregion
                #region Heal
                case Spell.Heal:
                    if (newPad.DPad.Right == ButtonState.Pressed && oldPad.DPad.Right == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.SkeletalGrasp;
                    }
                    if (newPad.DPad.Left == ButtonState.Pressed && oldPad.DPad.Left == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.FullCounter;
                    }

                    if (PlayerConditions[1])
                    {
                        //Playerints[4] =0;
                    }
                    else
                    {
                        Playerints[4] = 40;
                    }
                    break;
                #endregion
                #region Skeletal Grasp
                case Spell.SkeletalGrasp:
                    if (newPad.DPad.Right == ButtonState.Pressed && oldPad.DPad.Right == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.Knife;
                    }
                    if (newPad.DPad.Left == ButtonState.Pressed && oldPad.DPad.Left == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.Heal;
                    }
                    if (PlayerConditions[1])
                    {
                        Playerints[4] = 20;
                    }
                    else
                    {
                        Playerints[4] = 15;
                    }
                    break;
                #endregion
                #region Knife
                case Spell.Knife:
                    if (newPad.DPad.Right == ButtonState.Pressed && oldPad.DPad.Right == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.ConjureBats;
                    }
                    if (newPad.DPad.Left == ButtonState.Pressed && oldPad.DPad.Left == ButtonState.Released && CurrentState != PlayerStates.Casting)
                    {
                        CurrentSpell = Spell.SkeletalGrasp;
                    }
                    Playerints[4] = 0;
                    break;

                    #endregion

            }


            //this is for the player to use a spell or to throw a knife
            if (newPad.Buttons.Y == ButtonState.Pressed && oldPad.Buttons.Y == ButtonState.Released && CurrentState != PlayerStates.Casting && CurrentState != PlayerStates.Throwing)
            {
                //this checks if its a spell or the default ranged attack that is selected
                // if its not a spell then it would just go straight into the throwing state
                if (CurrentSpell != Spell.Knife)
                {
                    //this is used to make sure the blood trigger for the heal spell actually works properly
                    if (CurrentSpell != Spell.Heal)
                    {
                        if (Playerints[2] >= Playerints[4])
                        {
                            CurrentState = PlayerStates.Casting;
                            timer = 0f;
                            Playerints[5] = 0;
                            sourceRectangle.X = 0;

                        }

                    }
                    else
                    {
                        // this checks if the trigger for the blood magic is active or not
                        if (PlayerConditions[1] == false)
                        {
                            //this makes sure they meet the requirements to actually use the normal version of the heal spell
                            if (Playerints[2] >= Playerints[4])
                            {
                                if (Playerints[0] != Playerints[1])
                                {
                                    CurrentState = PlayerStates.Casting;
                                    timer = 0f;
                                    Playerints[5] = 0;
                                    sourceRectangle.X = 0;

                                }

                            }
                        }
                        else
                        {

                            CurrentState = PlayerStates.Casting;
                            timer = 0f;
                            Playerints[5] = 0;
                            sourceRectangle.X = 0;
                            Playerints[4] = Playerints[1] - Playerints[0];
                        }
                    }
                }
                else
                {
                    CurrentState = PlayerStates.Throwing;
                    timer = 0f;
                    Playerints[5] = 0;
                    sourceRectangle.X = 0;
                }
            }
            //this enables the blood trigger
            if (newPad.Buttons.RightShoulder == ButtonState.Pressed && oldPad.Buttons.RightShoulder == ButtonState.Released && CurrentState != PlayerStates.Casting)
            {
                if (PlayerConditions[1])
                {
                    PlayerConditions[1] = false;
                }
                else
                {
                    PlayerConditions[1] = true;
                }
            }

            foreach (Projectiles projectile in PlayerProjectiles)
            {
                for (int i = 0; i < EnemyList.Count - 1; i++)
                {
                    // this will feed a target in to the projectile and if there isn't a target on screen it will send the first entry in the array
                    if (EnemyList[i].COLLISION.Intersects(Screen) && projectile.OBTAINEDTARG == false)
                    {
                        projectile.getTarget(new Vector2(EnemyList[i].COLLISION.X + EnemyList[i].COLLISION.Width / 2, EnemyList[i].COLLISION.Y + EnemyList[i].COLLISION.Height / 2));
                        projectile.OBTAINEDTARG = true;
                    }
                    else if (projectile.OBTAINEDTARG == false)
                    {
                        projectile.getTarget(new Vector2(EnemyList[0].COLLISION.X + EnemyList[0].COLLISION.Width / 2, EnemyList[0].COLLISION.Y + EnemyList[0].COLLISION.Height / 2));
                        projectile.OBTAINEDTARG = true;
                    }

                }



                projectile.update(Screen, Tiles);
            }


            //this makes it so the players melee attack can hit the enemy
            if (PlayerConditions[0] == false)
            {
                DamageBox = Rectangle.Empty;
            }
            if (CurrentState != PlayerStates.Attack)
            {
                PlayerConditions[0] = false;
            }




            //players updates for all of its enums
            switch (CurrentState)
            {

                /*
                 * 
                 * anything with sourceRectangle.Y is setting what animation should be played
                 * 
                 */
                #region Attack
                case PlayerStates.Attack:

                    sourceRectangle.Y = sourceRectangle.Height * 2;

                    if (sourceRectangle.X >= texture.Width - sourceRectangle.Width)
                    {
                        PlayerConditions[0] = false;//makes it so the player cant damage the enemy outside of an attack
                        CurrentState = PlayerStates.Idle;
                    }
                    if (PlayerConditions[0])
                    {
                        if (FlipHori)
                        {
                            DamageBox = new Rectangle(rect.X - 20, rect.Y, 20, 74);
                        }
                        else
                        {
                            DamageBox = new Rectangle(rect.X + rect.Width, rect.Y, 20, 74);
                        }

                    }

                    break;
                #endregion

                #region Casting
                case PlayerStates.Casting:
                    Velocity.X = 0;
                    if (Playerints[0] - Playerints[4] <= 0 && PlayerConditions[1] && CurrentSpell != Spell.Heal)
                    {
                        CurrentState = PlayerStates.Idle;
                    }

                    switch (CurrentSpell)
                    {
                        #region Conjure Bats
                        case Spell.ConjureBats:
                            //this will do the spell once the animation hits this point
                            if (sourceRectangle.X >= texture.Width - sourceRectangle.Width)
                            {
                                //checks if the blood trigger is true or not then decides what one to do
                                if (!PlayerConditions[1])
                                {
                                    //resets the animation then depending on what direction the player is facing will create the projectile with the correct  movement and positioning requirements
                                    sourceRectangle.X = 0;
                                    if (!FlipHori)
                                    {
                                        PlayerProjectiles.Add(new BatProj(ProjectileText[0], (int)(Pos.X + rect.Width), (int)Pos.Y + sourceRectangle.Height / 4, 4, 8, 15, Tiles, FlipHori, ProjectileSounds[0]));
                                    }
                                    else
                                    {
                                        PlayerProjectiles.Add(new BatProj(ProjectileText[0], (int)Pos.X, (int)Pos.Y + sourceRectangle.Height / 4, 4, 8, 15, Tiles, FlipHori, ProjectileSounds[0]));
                                    }
                                    Playerints[2] -= Playerints[4]; // this takes the cost of the spell off of the current mana
                                    CurrentState = PlayerStates.Idle;
                                }
                                else
                                {
                                    timer += (float)gt.ElapsedGameTime.TotalSeconds;
                                    if (timer >= 0.2f)
                                    {
                                        if (!FlipHori)
                                        {
                                            PlayerProjectiles.Add(new BatProj(ProjectileText[0], (int)(Pos.X + rect.Width), (int)Pos.Y + sourceRectangle.Height / 4, 4, 8, 20, Tiles, FlipHori, ProjectileSounds[0]));
                                            timer = 0;
                                            Playerints[5]++;
                                        }
                                        else
                                        {
                                            PlayerProjectiles.Add(new BatProj(ProjectileText[0], (int)Pos.X, (int)Pos.Y + sourceRectangle.Height / 4, 4, 8, 20, Tiles, FlipHori, ProjectileSounds[0]));
                                            timer = 0;
                                            Playerints[5]++;
                                        }

                                    }
                                    if (Playerints[5] == 3)// this will exit out of the spell once it spawns 3 of the projectiles
                                    {
                                        CurrentState = PlayerStates.Idle;
                                        Playerints[0] -= 20;
                                        Playerints[2] -= Playerints[4];

                                    }

                                }
                            }
                            break;
                        #endregion
                        #region Full Counter

                        case Spell.FullCounter:

                            // same idea as the bats and will do it once it hits the end of the animation
                            if (sourceRectangle.X >= texture.Width - sourceRectangle.Width)
                            {
                                //checks the blood trigger
                                if (PlayerConditions[1])
                                {
                                    // will do this for upto the requirement of the timer
                                    if (timer < 0.8f)
                                    {
                                        timer += (float)gt.ElapsedGameTime.TotalSeconds;
                                        // this will place the counter rect in the correct side of the player depending on what way they're facing

                                        if (!FlipHori)
                                        {
                                            CounterRect = new Rectangle((int)Pos.X + rect.Width + 10, (int)Pos.Y, 20, rect.Height);

                                        }
                                        else
                                        {
                                            CounterRect = new Rectangle((int)Pos.X - 30, (int)Pos.Y, 20, rect.Height);
                                        }

                                        for (int i = 0; i < 3; i++) // adds particles in place of the counter rect to show its active
                                        {
                                            particles.Add(new Particle((int)CounterRect.X + (Game1.RNG.Next(0, CounterRect.Width)), (int)CounterRect.Y + (Game1.RNG.Next(0, CounterRect.Height)), 5));

                                        }
                                    }
                                    else
                                    {
                                        //resets them back to there idle state and removes a set amount of health for the blood trigger 
                                        CounterRect = Rectangle.Empty;
                                        Playerints[2] -= Playerints[4];
                                        Playerints[0] -= 10; 
                                        CurrentState = PlayerStates.Idle;
                                    }
                                }
                                else
                                {
                                    //same idea as the blood trigger but with less time
                                    if (timer < 0.4f)
                                    {
                                        timer += (float)gt.ElapsedGameTime.TotalSeconds;
                                        // this will place the counter rect in the correct side of the player depending on what way they're facing
                                        if (!FlipHori)
                                        {
                                            CounterRect = new Rectangle((int)Pos.X + rect.Width + 10, (int)Pos.Y, 20, rect.Height);
                                        }
                                        else
                                        {
                                            CounterRect = new Rectangle((int)Pos.X - 30, (int)Pos.Y, 20, rect.Height);
                                        }
                                        for (int i = 0; i < 3; i++) // adds particles in place of the counter rect to show its active
                                        {
                                            particles.Add(new Particle((int)CounterRect.X + (Game1.RNG.Next(0, CounterRect.Width)), (int)CounterRect.Y + (Game1.RNG.Next(0, CounterRect.Height)), 5));
                                        }
                                    }
                                    else
                                    {
                                        CounterRect = Rectangle.Empty;
                                        Playerints[2] -= Playerints[4];
                                        CurrentState = PlayerStates.Idle;
                                    }
                                }
                            }
                            break;

                        #endregion

                        #region Heal

                        case Spell.Heal:

                            if (Playerints[2] == 0)
                            {
                                CurrentState = PlayerStates.Idle;
                            }
                            //same idea as the previous two and will do this once its at the end of the animation
                            if (sourceRectangle.X >= texture.Width - sourceRectangle.Width)
                            {
                                // checks if the blood triger is false
                                if (!PlayerConditions[1])
                                {
                                    for (int i = 0; i < 20; i++) // adds partiles to show it activated
                                    {
                                        particles.Add(new Particle((int)Pos.X + (Game1.RNG.Next(0, sourceRectangle.Width)), (int)Pos.Y + (Game1.RNG.Next(0, sourceRectangle.Height / 2)), 5));
                                    }
                                    Playerints[0] += Playerints[4] / 2;
                                    Playerints[2] -= Playerints[4];
                                    CurrentState = PlayerStates.Idle;

                                }
                                else
                                {
                                    timer += (float)gt.ElapsedGameTime.TotalSeconds;
                                    if (timer > 1f) // will do the code once the timer hits this state
                                    {
                                        for (int i = 0; i < 20; i++)// adds particles to show it activated correctly
                                        {
                                            particles.Add(new Particle((int)Pos.X + (Game1.RNG.Next(0, sourceRectangle.Width)), (int)Pos.Y + (Game1.RNG.Next(0, sourceRectangle.Height / 2)), 5));
                                        }
                                        Playerints[0] += Playerints[2];
                                        Playerints[2] -= Playerints[4];
                                        CurrentState = PlayerStates.Idle;

                                    }
                                }

                            }
                            break;

                        #endregion
                        #region Skeletal Grasp
                        case Spell.SkeletalGrasp:
                            // if the player is falling it will cancel the spell and treat it as not happeneing
                            if (Velocity.Y != 0)
                            {
                                CurrentState = PlayerStates.Idle;
                            }
                            //this will trigger once the source rectangle gets to the end of the player animation
                            if (sourceRectangle.X >= texture.Width - sourceRectangle.Width)
                            {
                                //this determins if the blood trigger has been enabled
                                if (!PlayerConditions[1])
                                {
                                    //this determins what side of the player the projectile will spawn at
                                    if (!FlipHori)
                                    {
                                        //determins if they're on the bottom of a platform or not
                                        if (!FlipVert)
                                        {
                                            PlayerProjectiles.Add(new SkeletalGrasp(ProjectileText[3], (int)(Pos.X + rect.Width), (int)Pos.Y + sourceRectangle.Height - ProjectileText[3].Height / 2, 5, 12, 20, Tiles, FlipVert, null));
                                        }
                                        else
                                        {
                                            PlayerProjectiles.Add(new SkeletalGrasp(ProjectileText[3], (int)(Pos.X + rect.Width), (int)Pos.Y, 5, 12, 20, Tiles, FlipVert, null));

                                        }

                                    }
                                    else
                                    {
                                        if (!FlipVert)
                                        {
                                            PlayerProjectiles.Add(new SkeletalGrasp(ProjectileText[3], (int)(Pos.X - ProjectileText[3].Width / 8), (int)Pos.Y + sourceRectangle.Height - ProjectileText[3].Height / 2, 5, 12, 20, Tiles, FlipVert, null));
                                        }
                                        else
                                        {
                                            PlayerProjectiles.Add(new SkeletalGrasp(ProjectileText[3], (int)(Pos.X - ProjectileText[3].Width / 8), (int)Pos.Y, 5, 12, 20, Tiles, FlipVert, null));

                                        }

                                    }
                                    Playerints[2] -= Playerints[4];
                                    CurrentState = PlayerStates.Idle;
                                }
                                else
                                {
                                    //the time puts a slight delay between each one so they all don't spawn at once
                                    timer += (float)gt.ElapsedGameTime.TotalSeconds;
                                    if (timer >= 0.2f)
                                    {
                                        // same idea as the ones just above it
                                        if (!FlipHori)
                                        {
                                            if (!FlipVert)
                                            {
                                                PlayerProjectiles.Add(new SkeletalGrasp(ProjectileText[3], (int)(Pos.X + rect.Width) + ProjectileText[3].Width / 8 * Playerints[5], (int)Pos.Y + sourceRectangle.Height - ProjectileText[3].Height / 2, 5, 12, 20, Tiles, FlipVert, null));
                                            }
                                            else
                                            {
                                                PlayerProjectiles.Add(new SkeletalGrasp(ProjectileText[3], (int)(Pos.X + rect.Width) + ProjectileText[3].Width / 8 * Playerints[5], (int)Pos.Y, 5, 12, 20, Tiles, FlipVert, null));

                                            }

                                        }
                                        else
                                        {
                                            if (!FlipVert)
                                            {
                                                PlayerProjectiles.Add(new SkeletalGrasp(ProjectileText[3], (int)(Pos.X - ProjectileText[3].Width / 8) - ProjectileText[3].Width / 8 * Playerints[5], (int)Pos.Y + sourceRectangle.Height - ProjectileText[3].Height / 2, 5, 12, 20, Tiles, FlipVert, null));
                                            }
                                            else
                                            {
                                                PlayerProjectiles.Add(new SkeletalGrasp(ProjectileText[3], (int)(Pos.X - ProjectileText[3].Width / 8) - ProjectileText[3].Width / 8 * Playerints[5], (int)Pos.Y, 5, 12, 20, Tiles, FlipVert, null));

                                            }
                                        }
                                        // this resets the timer and increments into the next instance of it and creates the spacing between instance for the arms
                                        timer = 0;
                                        Playerints[5]++;
                                    }
                                    if (Playerints[5] == 3) // this will exit out of casting the attack
                                    {
                                        // this reverts the player back to the idle state 
                                        CurrentState = PlayerStates.Idle;
                                        Playerints[0] -= 10;
                                        Playerints[2] -= Playerints[4];

                                    }

                                }
                            }


                            break;
                            #endregion
                    }
                    // sets the animation for casting a spell.
                    sourceRectangle.Y = sourceRectangle.Height * 3;
                    break;
                #endregion

                #region dead

                case PlayerStates.Dead:
                    // this is to set the animation and thats it 
                    sourceRectangle.Y = texture.Height - sourceRectangle.Height;
                    break;
                #endregion

                #region hit
                    
                case PlayerStates.Hit:
                    //this resets their vertical position to normal so they can fall to the ground
                    FlipVert = false;
                    // this is all dependent on what side they have been hit on but sets their X velocity to gradually return to 0
                    if (Velocity.X > 0)
                    {
                        Velocity.X -= (float)gt.ElapsedGameTime.TotalSeconds * 2;
                    }
                    else
                    {
                        Velocity.X += (float)gt.ElapsedGameTime.TotalSeconds * 2;
                    }
                    if (Pos.X <= 0) // this was used in testing that they couldn't go off of the left side of the screen if there was no tile at that side of the map to stop them
                    {
                        Velocity.X = 0;
                        Pos.X = 0;
                    }
                    if (Velocity.X < 0.3f && Velocity.X > -0.3f)// once this codition has been met it resets the player back to their default state
                    {
                        Velocity.X = 0f;
                        PlayerConditions[2] = false;
                        CurrentState = PlayerStates.Idle;
                    }

                    // this is to fix an issue where the player would get stuck in a endless loop of getting hit
                    foreach (Enemies enemy in EnemyList)
                    {
                        if (rect.Intersects(enemy.COLLISION))
                        {
                            enemy.ApplyKnockBack(this, Tiles);
                            Velocity.X = 0;
                            if (rect.Center.X > enemy.COLLISION.Center.X)
                            {
                                Pos.X += 5;
                            }
                            else
                            {
                                Pos.X -= 5;
                            }
                        }
                    }

                    break;
                #endregion

                #region idle
                case PlayerStates.Idle:
                    if (newPad.ThumbSticks.Left.X != 0f && oldPad.ThumbSticks.Left.X != 0) // sets the player into the moving state
                    {
                        CurrentState = PlayerStates.Moving;
                    }
                    //this does the melee attack
                    if (newPad.Buttons.X == ButtonState.Pressed && oldPad.Buttons.X == ButtonState.Released)
                    {
                        sourceRectangle.X = 0;
                        PlayerConditions[0] = true;
                        CurrentState = PlayerStates.Attack;
                    }


                    //sets the animation to the idle animation
                    sourceRectangle.Y = sourceRectangle.Height * 0;
                    break;

                #endregion

                #region Move
                case PlayerStates.Moving:
                    if (newPad.ThumbSticks.Left.X == 0) // sets them to the idle state
                    {
                        CurrentState = PlayerStates.Idle;
                    }
                    if (newPad.Buttons.X == ButtonState.Pressed && oldPad.Buttons.X == ButtonState.Released) // sets the player state to attack when they press the button
                    {
                        sourceRectangle.X = 0;
                        PlayerConditions[0] = true;
                        CurrentState = PlayerStates.Attack;
                    }

                    if (Velocity.Y == 0) // this sets the player animation for walking
                    {
                        sourceRectangle.Y = sourceRectangle.Height;
                    }


                    break;
                #endregion

                #region Throwing

                case PlayerStates.Throwing:
                    //sets the animation for the throwing
                    sourceRectangle.Y = sourceRectangle.Height * 2;

                    //adds a projectile 
                    if (sourceRectangle.X >= texture.Width - sourceRectangle.Width)
                    {

                        if (!FlipHori)
                        {
                            PlayerProjectiles.Add(new KnifeProj(ProjectileText[2], (int)(Pos.X + rect.Width), (int)(Pos.Y + rect.Height / 2), 10, 1, Game1.RNG.Next(1, 6), Tiles, FlipHori, ProjectileSounds[1]));
                        }
                        else
                        {
                            PlayerProjectiles.Add(new KnifeProj(ProjectileText[2], (int)(Pos.X), (int)(Pos.Y + rect.Height / 2), -10, 1, Game1.RNG.Next(1, 6), Tiles, FlipHori, ProjectileSounds[1]));
                        }
                        PlayerConditions[0] = false;//makes it so the player cant damage the enemy outside of an attack
                        CurrentState = PlayerStates.Idle;
                    }

                    break;
                    #endregion
            }




            //this handles the mana recovery
            if (newPad.Triggers.Right > 0 && Playerints[0] > 1 && Playerints[2] != Playerints[3] && CurrentState != PlayerStates.Casting)
            {
                Playerints[2]++;
                Playerints[0]--;
                particles.Add(new Particle((int)Pos.X + (Game1.RNG.Next(0, sourceRectangle.Width)), (int)Pos.Y + (Game1.RNG.Next(0, sourceRectangle.Height / 2)), 5));
            }
            //this handles the particles that will be created on the player when doing certain actions
            foreach (Particle particle in particles)
            {
                particle.Update(gt);
            }
            //this handles the removal of the particles
            for (int J = 0; J < particles.Count; J++)
            {

                if (particles[J].VisibleItIs == false)
                {
                    particles.RemoveAt(J);

                }

            }
            //this puts a limit on the number of particles to reduce the frame drop
            if (particles.Count > 100)
            {
                particles.RemoveAt(0);
            }

            //this handles the player projectile removal
            for (int P = PlayerProjectiles.Count - 1; P >= 0; P--)
            {
                if (PlayerProjectiles[P].VisibleItIs == false)
                {
                    PlayerProjectiles.RemoveAt(P);
                    break;
                }

            }
            //this adds particles to the dead enemies
            for (int E = 0; E < EnemyList.Count; E++)
            {
                if (EnemyList[E].EnAction == EnemyAction.Death && EnemyList[E].TIMER < 1)
                {
                    Playerints[2]++;
                }
                // this handles the effects of the full counter spell
                foreach (Projectiles enemyProj in EnemyList[E].THESEWILLHURTPROJ)
                {
                    if (enemyProj.COLLISION.Intersects(CounterRect))
                    {
                        //checks if its an arrow that has hit it
                        if (enemyProj.TYPEPROJ == "arrow")
                        {
                            if (!FlipHori)
                            {
                                if (!PlayerConditions[1])
                                    PlayerProjectiles.Add(new ArrowProj(ProjectileText[1], (int)Pos.X, (int)Pos.Y + rect.Height / 3, 5, 1, enemyProj.DAMAGE, Tiles, false, ProjectileSounds[1]));
                                else
                                    PlayerProjectiles.Add(new ArrowProj(ProjectileText[1], (int)Pos.X, (int)Pos.Y + rect.Height / 3, 5, 1, enemyProj.DAMAGE * 2, Tiles, false, ProjectileSounds[1]));

                            }
                            else
                            {
                                if (!PlayerConditions[1])
                                    PlayerProjectiles.Add(new ArrowProj(ProjectileText[1], (int)Pos.X, (int)Pos.Y + rect.Height / 3, -5, 1, enemyProj.DAMAGE, Tiles, true, ProjectileSounds[1]));
                                else
                                    PlayerProjectiles.Add(new ArrowProj(ProjectileText[1], (int)Pos.X, (int)Pos.Y + rect.Height / 3, -5, 1, enemyProj.DAMAGE * 2, Tiles, true, ProjectileSounds[1]));

                            }
                        }
                        //checks if it is a bolt that has hit it 
                        if (enemyProj.TYPEPROJ == "bolt")
                        {
                            if (!FlipHori)
                            {
                                if (!PlayerConditions[1])
                                {

                                    PlayerProjectiles.Add(new BoltOfLight(ProjectileText[4], (int)Pos.X + rect.Width + enemyProj.COLLISION.Width, (int)Pos.Y + rect.Height / 3, 8, 4, enemyProj.DAMAGE, Tiles, true, ProjectileSounds[2]));
                                }
                                else
                                {

                                    PlayerProjectiles.Add(new BoltOfLight(ProjectileText[4], (int)Pos.X + rect.Width + enemyProj.COLLISION.Width, (int)Pos.Y + rect.Height / 3, 8, 4, enemyProj.DAMAGE * 2, Tiles, true, ProjectileSounds[2]));
                                }
                                PlayerProjectiles[PlayerProjectiles.Count - 1].getTarget(new Vector2(EnemyList[E].COLLISION.Center.X, EnemyList[E].COLLISION.Center.Y));
                                PlayerProjectiles[PlayerProjectiles.Count - 1].OBTAINEDTARG = true;
                            }
                            else
                            {
                                if (!PlayerConditions[1])
                                    PlayerProjectiles.Add(new BoltOfLight(ProjectileText[4], (int)Pos.X - enemyProj.COLLISION.Width, (int)Pos.Y + rect.Height / 3, -8, 4, enemyProj.DAMAGE, Tiles, true, ProjectileSounds[2]));
                                else
                                    PlayerProjectiles.Add(new BoltOfLight(ProjectileText[4], (int)Pos.X - enemyProj.COLLISION.Width, (int)Pos.Y + rect.Height / 3, -8, 4, enemyProj.DAMAGE * 2, Tiles, true, ProjectileSounds[2]));

                            }
                        }

                        enemyProj.VisibleItIs = false;


                    }
                    //this will add the velocity to the pplayer when they're hit by the projectile of the enemy
                    if (enemyProj.COLLISION.Intersects(rect) && CurrentState != PlayerStates.Dead)
                    {
                        HitByProjectil(enemyProj);
                    }
                }
                // this will add particles in the place where the enemy dies
                if ((EnemyList[E].EnAction == EnemyAction.Death || EnemyList[E].BossMove == BossAction.Death) && EnemyList[E].TIMER > 5)
                {
                    particles.Add(new Particle(Game1.RNG.Next(EnemyList[E].COLLISION.X, EnemyList[E].COLLISION.X + EnemyList[E].COLLISION.Width), Game1.RNG.Next(EnemyList[E].COLLISION.Y, EnemyList[E].COLLISION.Y + EnemyList[E].COLLISION.Width / 2), 5));
                }
            }
            //this switches the player to the death state
            if (Playerints[0] <= 0 && CurrentState != PlayerStates.Dead)
            {
                sourceRectangle.X = 0;
                sourceRectangle.Y = texture.Height - sourceRectangle.Height;
                CurrentState = PlayerStates.Dead;
            }

            // this just keeps all the move code in one place
            // This is placed at the bottom so it can overwrite some code at the top if needs to like setting the source rectangle position
            Collisions(newPad, oldPad, Tiles, EnemyList, gt);
            Move();

            // this makes it so the animation goes back to the default one if they are falling or jumping
            if (Velocity.Y != 0)
            {
                if (CurrentState == PlayerStates.Dead)
                {
                }
                else
                {
                    foreach (CollisionTiles tile in Tiles)
                    {
                        if (!rect.TouchTopof(tile.Rectangle) && CurrentState == PlayerStates.Moving)
                        {
                            sourceRectangle.Y = 0;

                        }
                    }

                }
            }
        }
        /// <summary>
        /// the whole purpose of this is so the player actually takes damage from the enemy projectile and is called in the enemy whenever they get hit
        /// </summary>

        public void HitByProjectil(Projectiles enemyProj)
        {
            Velocity.Y = -12;
            if (Pos.X < enemyProj.COLLISION.X)
            {
                Velocity.X = -1.2f;
                FlipHori = false;
            }
            else
            {
                Velocity.X = 1.2f;
                FlipHori = true;
            }
            PlayerConditions[2] = true;
            Playerints[0] -= enemyProj.DAMAGE;
            CurrentState = PlayerStates.Hit;

            enemyProj.VisibleItIs = false;
        }

        public override void Draw(SpriteBatch sb, GameTime gt)
        {


            //this draws the player texture 
            switch (CurrentState)
            {
                case PlayerStates.Attack:

                    Drawing(sb, gt, 12);

                    break;

                case PlayerStates.Casting:

                    Drawing(sb, gt, 8);

                    break;
                case PlayerStates.Dead:

                    Drawing(sb, gt, 2);

                    break;
                case PlayerStates.Hit:

                    if (!FlipHori)
                    {
                        sb.Draw(HitTexture, rect, Color.Red);
                    }
                    else
                    {
                        sb.Draw(HitTexture, rect, null, Color.Red, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
                    }

                    break;

                case PlayerStates.Idle:

                    Drawing(sb, gt, 6);

                    break;


                case PlayerStates.Moving:

                    Drawing(sb, gt, 5);

                    break;
                case PlayerStates.Throwing:

                    Drawing(sb, gt, 16);

                    break;

            }
            foreach (Particle particle in particles)
            {
                //draws the particles
                particle.Drawing(sb);
            }
            foreach (Projectiles projectile in PlayerProjectiles)
            {
                //draws the projectiles
                projectile.Draw(sb, gt);
            }

#if DEBUG
            sb.Draw(Game1.debugPixel, rect, Color.DarkBlue * 0.5f);
            sb.DrawString(Game1.debugFont, "Pos: " + Pos + "\nVelocity: " + Velocity + "\nRect: " + rect +
                          "\nCurrent State:" + CurrentState +
                          "\nCurrent Spell:" + CurrentSpell +
                          "\nParticle Count: " + PlayerProjectiles.Count +
                          "\n Jumped: " + PlayerConditions[3] +
                          "\n Timer: " + timer, Pos, Color.White);
            sb.Draw(Game1.debugPixel, DamageBox, Color.Purple * 0.5f);
            sb.Draw(Game1.debugPixel, CounterRect, Color.Yellow * 0.5f);

#endif
        }


        /// <summary>
        ///
        ///             this is used for updating the animation for the player and will adjust the speed that each one is played at depending what one is selected
        /// 
        /// </summary>
   
        void UpdateTrigger(int Frames, GameTime gt)
        {
            updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * Frames;

            if (updateTrigger >= 1)
            {
                updateTrigger = 0;
                sourceRectangle.X += sourceRectangle.Width;
                if (sourceRectangle.X >= texture.Width)
                {
                    if (PlayerConditions[1] == true && CurrentState == PlayerStates.Casting)
                    {
                        sourceRectangle.X = texture.Width - sourceRectangle.Width;
                    }
                    else if (CurrentState == PlayerStates.Dead)
                    {
                        sourceRectangle.X = texture.Width - sourceRectangle.Width;
                    }
                    else if (CurrentSpell == Spell.FullCounter && CurrentState == PlayerStates.Casting)
                    {
                        sourceRectangle.X = texture.Width - sourceRectangle.Width;
                    }
                    else
                    {
                        sourceRectangle.X = 0;
                    }
                }

            }
        }

        /// <summary>
        /// 
        ///         this handles the players collisions with the tiles along with when they actually intersect with the enemy
        /// 
        /// </summary>
        /// <param name="newPad"></param>
        /// <param name="oldPad"></param>
        /// <param name="Tiles"></param>
        /// <param name="EnemyList"></param>
        /// <param name="gt"></param>
        void Collisions(GamePadState newPad, GamePadState oldPad, List<CollisionTiles> Tiles, List<Enemies> EnemyList, GameTime gt)
        {
            //changes the player gravity if up or down is pressed on the d-pad
            if (newPad.DPad.Up == ButtonState.Pressed && oldPad.DPad.Up == ButtonState.Released && !FlipVert && CurrentState != PlayerStates.Hit && CurrentState != PlayerStates.Dead && Velocity.Y == 0)
            {

                FlipVert = true;
            }
            if (newPad.DPad.Down == ButtonState.Pressed && oldPad.DPad.Down == ButtonState.Released && FlipVert && CurrentState != PlayerStates.Hit && CurrentState != PlayerStates.Dead && Velocity.Y == 0)
            {

                FlipVert = false;

            }
            //this means the player can move if they haven't been hit
            if (!PlayerConditions[2])
            {
                if (CurrentState != PlayerStates.Casting && CurrentState != PlayerStates.Dead)
                {
                    if (Pos.X < 0)
                    {

                        if (newPad.ThumbSticks.Left.X > 0)
                        {
                            Velocity.X = newPad.ThumbSticks.Left.X * Speed;
                        }
                        else
                        {
                            Velocity.X = 0;
                            sourceRectangle.Y = 0;
                        }
                    }
                    else
                    {
                        Velocity.X = newPad.ThumbSticks.Left.X * Speed;
                    }
                }
            }

            // if the player hasn't died then this will happen
            if (CurrentState != PlayerStates.Dead)
            {
                // applies positive or negative gravity depending on what state its in
                if (!FlipVert)
                {
                    Velocity.Y += 0.75f;
                }
                else
                {
                    Velocity.Y -= 0.75f;
                    flippedTimer += (float)gt.ElapsedGameTime.TotalSeconds;
                }
                if (Velocity.Y > 15)// cant exceed this ammount
                {
                    Velocity.Y = 15;
                }
                if (Velocity.Y < -15)// cant exceed this amount
                {
                    Velocity.Y = -15;
                }
                if (flippedTimer > 0.45) // will reset the players gravity to normal if they fall upwards for too long
                {

                    FlipVert = false;
                }

                // this makes sure that the player can do the double jump(only when the gravity is applied normally and not flipped)
                if (Velocity.Y < 0)
                {
                    if (newPad.Buttons.A == ButtonState.Pressed && oldPad.Buttons.A == ButtonState.Released && !second)
                    {
                        if (CurrentState != PlayerStates.Casting)
                        {
                            second = true;
                            if (!FlipVert)
                            {

                                Velocity.Y -= 7.5f;
                            }
                            else
                            {
                                Velocity.Y += 7.5f;
                            }
                            PlayerConditions[3] = true;
                        }

                    }
                }
            }

            // this is used for drawing it in the correct direction as far as i can remember
            if (Velocity.X > 0 && !PlayerConditions[2])
            {
                FlipHori = false;
            }
            if (Velocity.X < 0 && !PlayerConditions[2])
            {
                FlipHori = true;
            }

            // used for removing a particle if it is inside a tile
            foreach (CollisionTiles tile in Tiles)
            {
                for (int P = 0; P < particles.Count; P++)
                {
                    if (tile.Rectangle.Contains(particles[P].POS))
                    {
                        particles[P].VisibleItIs = false;

                    }
                }


                //this handles the collision of the player against the top of a tile
                if (rect.TouchTopof(tile.Rectangle) && PlayerConditions[3])
                {
                    if (!FlipVert)
                    {
                        PlayerConditions[3] = false;
                        Velocity.Y = 0;
                        flippedTimer = 0;
                        Pos.Y = tile.Rectangle.Y - sourceRectangle.Height;
                    }
                    else
                    {
                        PlayerConditions[3] = false;
                        Velocity.Y = -1;
                        flippedTimer = 0;
                        Pos.Y = tile.Rectangle.Top - rect.Height - 5;
                    }

                }
                // the bottom of the tile when the player hits it
                if (rect.TouchBottomof(tile.Rectangle))
                {
                    if (FlipVert)
                    {
                        PlayerConditions[3] = false;
                        Velocity.Y = 0;
                        flippedTimer = 0;
                        Pos.Y = tile.Rectangle.Bottom;
                    }
                    else
                    {
                        PlayerConditions[3] = false;
                        Velocity.Y = 1;
                        flippedTimer = 0;
                        Pos.Y = tile.Rectangle.Bottom + 5;

                    }

                }
                // this is used to make the player actually jump and enable the codition to double jump
                if (newPad.Buttons.A == ButtonState.Pressed && oldPad.Buttons.A == ButtonState.Released && PlayerConditions[3] == false)
                {
                    if (CurrentState != PlayerStates.Casting)
                    {
                        second = false;
                        if (!FlipVert)
                        {
                            flippedTimer = 0;
                            Velocity.Y = -15;
                        }
                        else
                        {
                            flippedTimer = 0;
                            Velocity.Y = 15;
                        }
                        PlayerConditions[3] = true;
                    }

                }
                //means they can jump again
                if (!rect.TouchTopof(tile.Rectangle) && PlayerConditions[3] == false)
                {
                    PlayerConditions[3] = true;
                }




                //this handles when the player touches the left side of a tile
                if (rect.TouchLeftof(tile.Rectangle))
                {
                    if (Velocity.X < 0)
                    {

                    }
                    else
                    {
                        Velocity.X = 0;
                        sourceRectangle.Y = 0;
                        Pos.X = tile.Rectangle.Left - sourceRectangle.Width;
                    }
                }
                //this is for the right
                if (rect.TouchRightof(tile.Rectangle))
                {
                    if (Velocity.X > 0)
                    {

                    }
                    else
                    {
                        Velocity.X = 0;
                        sourceRectangle.Y = 0;
                        Pos.X = tile.Rectangle.Right;
                    }
                }

            }
            foreach (Enemies enemy in EnemyList)
            {
                // this makes the player face the direction they were hit from
                if ((rect.Intersects(enemy.COLLISION) || rect.Intersects(enemy.PlayerHit)) && PlayerConditions[2] == false)
                {
                    //makes sure they don't get treated as hit when the enemy is actually dead
                    if (enemy.EnAction == EnemyAction.Death)
                    {

                    }
                    else
                    {
                        Velocity.Y = -12;
                        if (Pos.X < enemy.COLLISION.X)
                        {
                            Velocity.X = -2;
                            FlipHori = false;
                        }
                        else
                        {
                            Velocity.X = 2;
                            FlipHori = true;
                        }
                        Playerints[0] -= enemy.DAMAGE;
                        PlayerConditions[2] = true;
                        CurrentState = PlayerStates.Hit;

                    }
                }
            }
        }

        /// <summary>
        /// 
        ///             this is used to draw the player properly 
        ///             
        ///             this has the frames fed into it as well as this is where the update trigger is used to animate the player
        /// 
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="gt"></param>
        /// <param name="Frames"></param>
        void Drawing(SpriteBatch sb, GameTime gt, int Frames)
        {
            // this is for drawing the player for when they are moving right
            UpdateTrigger(Frames, gt);
            if (!FlipHori)
            {
                if (FlipVert == false)
                {
                    sb.Draw(texture, rect, sourceRectangle, Color.White);
                }
                else
                {
                    sb.Draw(texture, rect, sourceRectangle, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
                }
            }
            else
            {
                if (FlipVert == false)
                {
                    sb.Draw(texture, rect, sourceRectangle, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
                }
                else
                {
                    sb.Draw(texture, rect, sourceRectangle, Color.White, 0f, Vector2.Zero, DoubleFlip, 0f);
                }
            }

        }

        /// <summary>
        /// does the movement of the player and that's it
        /// </summary>
        void Move()
        {
            #region Movement of the player




            Pos.X += Velocity.X * Speed;
            Pos.Y += Velocity.Y;


            rect.X = (int)Pos.X;
            rect.Y = (int)Pos.Y;
            #endregion
        }

        /// <summary>
        ///             This draws in all the player hud elements and is seperate from the player drawing
        ///             
        ///             the health bar will scale in size depnding on the max health that the player has left
        ///             
        ///             the way that the mana is set up is similar to the health but isn't actually increased during the game but is setup so it can be used
        ///             
        ///             this is also where it will draw the ability selected along with the button to use it
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="HealthBar"></param>
        /// <param name="ManaBar"></param>
        /// <param name="OuterBar"></param>
        /// <param name="BarEndCap"></param>
        /// <param name="HudFont"></param>
        public void DrawHud(SpriteBatch sb, Texture2D HealthBar, Texture2D ManaBar, Texture2D OuterBar, Texture2D BarEndCap, SpriteFont HudFont)
        {
            #region Condions on health and mana
            // this is so the health and mana can't go outside of its parameters

            if (Playerints[0] > Playerints[1])
            {
                Playerints[0] = Playerints[1];
            }
            if (Playerints[2] > Playerints[3])
            {
                Playerints[2] = Playerints[3];
            }
            #endregion

            // This is the Health Bar getting drawn 
            sb.Draw(BarEndCap, new Rectangle(3, 3, BarEndCap.Width, BarEndCap.Height), Color.White);
            sb.Draw(OuterBar, new Rectangle(3 + BarEndCap.Width, 3, Playerints[1], OuterBar.Height), Color.White);
            sb.Draw(HealthBar, new Rectangle(6, 5, Playerints[0], HealthBar.Height), Color.White);
            sb.Draw(BarEndCap, new Rectangle(3 + Playerints[1], 3, BarEndCap.Width, BarEndCap.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            sb.DrawString(HudFont, Playerints[0] + " / " + Playerints[1], new Vector2(20, 10), Color.White);

            //this is the mana bar getting drawn
            sb.Draw(BarEndCap, new Rectangle(3, 6 + BarEndCap.Height, BarEndCap.Width, BarEndCap.Height), Color.White);
            sb.Draw(OuterBar, new Rectangle(3 + BarEndCap.Width, 6 + BarEndCap.Height, Playerints[3], OuterBar.Height), Color.White);
            sb.Draw(ManaBar, new Rectangle(6, 8 + BarEndCap.Height, Playerints[2], HealthBar.Height), Color.White);
            sb.Draw(BarEndCap, new Rectangle(3 + Playerints[3], 6 + BarEndCap.Height, BarEndCap.Width, BarEndCap.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            sb.DrawString(HudFont, Playerints[2] + " / " + Playerints[3], new Vector2(20, 52), Color.White);
            switch (CurrentSpell)
            {
                case Spell.ConjureBats:
                    if (!PlayerConditions[1])
                        sb.Draw(ProjectileText[0], new Rectangle(Playerints[1] + 30, 10, ProjectileText[0].Width / 4, ProjectileText[0].Height), new Rectangle(0, 0, ProjectileText[0].Width / 4, ProjectileText[0].Height), Color.White);
                    else
                        sb.Draw(ProjectileText[0], new Rectangle(Playerints[1] + 30, 10, ProjectileText[0].Width / 4, ProjectileText[0].Height), new Rectangle(0, 0, ProjectileText[0].Width / 4, ProjectileText[0].Height), Color.Red);

                    break;
                case Spell.FullCounter:
                    if (!PlayerConditions[1])
                        sb.Draw(ProjectileText[6], new Rectangle(Playerints[1] + 30, 10, ProjectileText[6].Width / 2, ProjectileText[6].Height / 2), Color.Cyan);
                    else
                        sb.Draw(ProjectileText[6], new Rectangle(Playerints[1] + 30, 10, ProjectileText[6].Width / 2, ProjectileText[6].Height / 2), Color.Red);

                    break;
                case Spell.SkeletalGrasp:
                    if (!PlayerConditions[1])
                        sb.Draw(ProjectileText[3], new Rectangle(Playerints[1] + 25, 0, ProjectileText[3].Width / 8, ProjectileText[3].Height / 2), new Rectangle(ProjectileText[3].Width / 4 * 3, 0, ProjectileText[3].Width / 4, ProjectileText[3].Height), Color.White);
                    else
                        sb.Draw(ProjectileText[3], new Rectangle(Playerints[1] + 25, 0, ProjectileText[3].Width / 8, ProjectileText[3].Height / 2), new Rectangle(ProjectileText[3].Width / 4 * 3, 0, ProjectileText[3].Width / 4, ProjectileText[3].Height), Color.Red);

                    break;
                case Spell.Heal:
                    if (!PlayerConditions[1])
                        sb.Draw(ProjectileText[5], new Rectangle(Playerints[1] + 30, 10, ProjectileText[5].Width / 2, ProjectileText[5].Height / 2), Color.LimeGreen);
                    else
                        sb.Draw(ProjectileText[5], new Rectangle(Playerints[1] + 30, 10, ProjectileText[5].Width / 2, ProjectileText[5].Height / 2), Color.Red);

                    break;
                case Spell.Knife:
                    sb.Draw(ProjectileText[2], new Rectangle(Playerints[1] + 30, 20, (int)(ProjectileText[2].Width / 1.25f), (int)(ProjectileText[2].Height / 1.25f)), Color.White);
                    break;
            }
            // this highlights the button on the screen that the player should press to use a ranged attack
            if (Playerints[2] >= Playerints[4])
            {
                sb.Draw(ProjectileText[7], new Rectangle(Playerints[1] + 50, 40, (int)(ProjectileText[7].Width / 2.5), (int)(ProjectileText[7].Height / 2.5f)), Color.White);

            }
            else
            {
                sb.Draw(ProjectileText[7], new Rectangle(Playerints[1] + 50, 40, (int)(ProjectileText[7].Width / 2.5), (int)(ProjectileText[7].Height / 2.5f)), Color.White * 0.5f);

            }
        }

        //this does all the camera movement and follows the player
        public Vector2 CameraMovement(int X, int Y, LEVEL CURRENT, Map CurrentLevel)
        {
            if (CURRENT != LEVEL.Boss1)
            {
                //this basically means the camera wont move when the player is at the far left
                if (Pos.X < X / 2)
                {
                    // this sets the camera Y cords depending on their relative height 
                    if (Pos.Y < Y / 2)
                    {
                        return new Vector2(0, 0);

                    }
                    else if (Pos.Y > CurrentLevel.Height - Y / 2)
                    {
                        return new Vector2(0, -CurrentLevel.Height + Y);

                    }
                    else
                    {

                        return new Vector2(0, -Pos.Y + (Y / 2));
                    }
                }
                else if (Pos.X > CurrentLevel.Width - X / 2)
                {
                    if (Pos.Y < Y / 2)
                    {
                        return new Vector2(-CurrentLevel.Width + X, 0);

                    }
                    else if (Pos.Y > CurrentLevel.Height - Y / 2)
                    {
                        return new Vector2(-CurrentLevel.Width + X, -CurrentLevel.Height + Y);

                    }
                    else
                    {

                        return new Vector2(-CurrentLevel.Width + X, -Pos.Y + (Y / 2));
                    }
                }
                else
                {

                    if (Pos.Y < Y / 2)
                    {
                        return new Vector2((-Pos.X + (X / 2)), 0);

                    }
                    else if (Pos.Y > CurrentLevel.Height - Y / 2)
                    {
                        return new Vector2((-Pos.X + (X / 2)), -CurrentLevel.Height + Y);

                    }
                    else
                    {

                        return new Vector2((-Pos.X + (X / 2)), (-Pos.Y + (Y / 2)));


                    }
                }

            }
            else
            {
                //basically if it's the boss room the camera will not move
                return Vector2.Zero;
            }

        }

    }

    /// <summary>
    ///     this sets up the health upgrades for upgrading the players max health
    /// </summary>
    class HealthUpgrades : StaticGraphic
    {
        private bool isVisible;
        public bool VISIBLE
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
            }
        }
        public HealthUpgrades(Texture2D txr, int X, int Y) : base(txr, X, Y)
        {
            rect = new Rectangle(X, Y, texture.Width, texture.Height);
            isVisible = true;
        }
    }
}
