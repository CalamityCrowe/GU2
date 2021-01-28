using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;


namespace TwistedSoul
{
    enum EnemyAction
    {
        Movement,
        Attack,
        Death,
        Hit
    }
    class Enemies : AnimatedGraphics
    {

        protected Texture2D ProjectileTexture;
        protected EnemyAction CurrentAction;
        public EnemyAction EnAction
        {
            get
            {
                return CurrentAction;
            }
        }
        protected BossAction CurrentMove;
        public BossAction BossMove
        {
            get
            {
                return CurrentMove;
            }
        }
        protected int Health;
        protected int[] ACTIONSCOUNTER = new int[2];

        //this is used by the ranged enemy and boss for the creation of their projectiles
        protected int Projectile_Count = 0;
        protected bool Seeking;
        protected Rectangle Detection;
        protected SoundEffect ProjWalksound; // refered to this as only one of the enemies will use a walk sound and the rest shall use sounds for projectiles
        protected SoundEffectInstance WalkInstance;
        protected bool CanTakeDamage = true;
        protected Rectangle HitRect;
        protected List<Projectiles> EnemyPoj;

        protected float ActivationTimer;
        public Rectangle PlayerHit
        {
            get
            {
                return HitRect;
            }
        }
        protected float Timer, Colour;
        public float TIMER
        {
            get
            {
                return Timer;
            }
        } //used with the player to indicate when to add particles around the enemy after they have been dead for a few seconds
        public List<Projectiles> THESEWILLHURTPROJ
        {
            get
            {
                return EnemyPoj;
            }
        }

        public Enemies(Texture2D txr, int X, int Y, float S, int frames, int D, int H, bool flip, SoundEffect FootSteps) : base(txr, X, Y, S, frames, D, flip)
        {
            Health = H;

            Colour = 1;

            isVisible = true;
            EnemyPoj = new List<Projectiles>();

            ProjWalksound = FootSteps;
            WalkInstance = ProjWalksound.CreateInstance();
            WalkInstance.IsLooped = true;
        }
        /// <summary>
        ///         this whole method is to fix a frame rate issue when there's too many enemies in the level
        ///         basically if they aren't on screen then this will start a timer and once it reaches 4.5 seconds it will call their update function a couple of times before it resets the timer
        ///         this is so it gives the illusion that the enemies that aren't on screen are doing something and not look like they've just spawned in
        ///         this will also keep the enemy projectile moving when they aren't on the screen
        /// </summary>
        public void BackgroundUpdate(List<CollisionTiles> Tiles, Player CurrentPlayer, Rectangle Screen, GameTime gt, GameState CurrState)
        {
            STOPSOUND();
            foreach (Projectiles projectile in EnemyPoj)
            {
                projectile.Update();
            }
            ActivationTimer += (float)gt.ElapsedGameTime.TotalSeconds;
            if (ActivationTimer >= 4.5f)
            {
                Update(Tiles, CurrentPlayer, Screen, gt, CurrState);
            }
            if (ActivationTimer >= 5)
            {
                ActivationTimer = 0;
            }
        }
        // this is for drawing the projectiles still whilst the enemy that made them is off screen
        public void OffscreenDraw(SpriteBatch sb, GameTime gt)
        {
            foreach (Projectiles projectile in EnemyPoj)
            {
                projectile.Draw(sb, gt);
            }
        }
        //this is used in the player update to essentially push the enemy away when the player is currently taking damage
        public void ApplyKnockBack(Player CurrentPlayer, List<CollisionTiles> Tiles)
        {
            if (CurrentPlayer.COLLISION.Center.X > rect.Center.X)
            {
                Velocity.X = -2;
            }
            else
            {
                Velocity.X = 2;
            }
            // this makes both types set to hit for their action so the neither of them can slide off the screen
            CurrentAction = EnemyAction.Hit;
            CurrentMove = BossAction.Hit; //
        }
        public virtual void Update(List<CollisionTiles> Tiles, Player CurrentPlayer, Rectangle Screen, GameTime gt, GameState CurrState)
        {
            // this basically means they can recieve damage when they get hit
            if (CurrentAction == EnemyAction.Attack || CurrentAction == EnemyAction.Movement)
            {
                CanTakeDamage = true;
            }
            // this is the vertical gravity contantly getting applied
            Velocity.Y += 1;
            if (Velocity.Y > 12)
            {
                Velocity.Y = 12;
            }
            //this stops the verticle gravity when they touch the top of a tile
            foreach (CollisionTiles tile in Tiles)
            {
                if (rect.TouchTopof(tile.Rectangle))
                {
                    Velocity.Y = 0;
                    Pos.Y = tile.Rectangle.Y - rect.Height;
                }

            }
            //this means they can be hit by the players melee attack
            if (rect.Intersects(CurrentPlayer.HIT) && CurrentAction != EnemyAction.Death && CanTakeDamage == true)
            {
                if (CurrentPlayer.PlayerPos.X < Pos.X)
                {
                    Velocity.X = 3;
                }
                else
                {
                    Velocity.X = -3;
                }

                Velocity.Y = -5;
                Health -= CurrentPlayer.DAMAGE;
                CurrentAction = EnemyAction.Hit;
                CanTakeDamage = false;
            }
            for (int p = 0; p < CurrentPlayer.PLAYERPROJECTILES.Count; p++)
            {
                //this does the damage from the player projectiles
                if (CurrentPlayer.PLAYERPROJECTILES[p].COLLISION.Intersects(rect) && CurrentPlayer.PLAYERPROJECTILES[p].VisibleItIs && CanTakeDamage)
                {
                    // if its a knife that hits the enemy the knock back won't be applied
                    if (CurrentPlayer.PLAYERPROJECTILES[p].TYPEPROJ != "knife")
                    {
                        //applies an x velocity depending on what side they were hit from
                        if (CurrentPlayer.PlayerPos.X < Pos.X)
                        {
                            Velocity.X = 3;
                        }
                        else
                        {
                            Velocity.X = -3;
                        }
                        Velocity.Y = -5;
                        if (!Seeking)// if the player hasn't been spotted they will do slightly more damage
                        {
                            Health -= (int)(CurrentPlayer.PLAYERPROJECTILES[p].DAMAGE * 1.5f);
                        }
                        else
                        {
                            Health -= CurrentPlayer.PLAYERPROJECTILES[p].DAMAGE;

                        }
                        CurrentPlayer.PLAYERPROJECTILES[p].VisibleItIs = false;
                        Seeking = true;
                        Velocity.Y = -5;

                        CurrentAction = EnemyAction.Hit;

                    }
                    else
                    {
                        if (!Seeking)
                        {
                            Health -= (int)(CurrentPlayer.PLAYERPROJECTILES[p].DAMAGE * 1.5f);
                        }
                        else
                        {
                            Health -= CurrentPlayer.PLAYERPROJECTILES[p].DAMAGE;

                        }
                        CurrentPlayer.PLAYERPROJECTILES[p].VisibleItIs = false;
                        Seeking = true;
                    }
                }
            }
            // this kills the enemy 
            if (CurrentAction != EnemyAction.Death)
            {
                if (Health <= 0)
                {
                    CurrentAction = EnemyAction.Death;
                    Velocity.X = 0;
                    WalkInstance.Stop();
                    sourceRectangle.X = 0;
                }
            }
            //this damages the player
            if (HitRect.Intersects(CurrentPlayer.COLLISION) && CurrentPlayer.STATE != PlayerStates.Hit)
            {
                CurrentPlayer.HEALTH -= Damage;

            }

            if (CurrentAction == EnemyAction.Hit)
            {

                // this slows the them down so they can revert back to their previous state
                if (Velocity.X > 0)
                {
                    Velocity.X -= (float)gt.ElapsedGameTime.TotalSeconds * 3;
                }
                else
                {
                    Velocity.X += (float)gt.ElapsedGameTime.TotalSeconds * 3;
                }
                //this resets any of the enemies back to the moveing state 
                if ((int)Velocity.X == 0)
                {
                    CanTakeDamage = true;
                    Velocity.X = 0;
                    CurrentAction = EnemyAction.Movement;
                }
                foreach (CollisionTiles tile in Tiles)
                {
                    // this stops the enemy when they intersect with the tiles sides whilst in the hit state
                    if (rect.TouchLeftof(tile.Rectangle))
                    {
                        if (Velocity.X > 0)
                        {
                            Velocity.X = 0;
                        }
                        else
                        {

                        }
                    }
                    if (rect.TouchRightof(tile.Rectangle))
                    {
                        if (Velocity.X < 0)
                        {
                            Velocity.X = 0;
                        }
                        else
                        {

                        }
                    }
                }
            }
            //this will make the enemy die 
            if (CurrentAction == EnemyAction.Death)
            {
                CanTakeDamage = false;
                Timer += (float)gt.ElapsedGameTime.TotalSeconds;

                if (Timer >= 5)
                {
                    Colour -= 0.005f;
                }
                if (Colour <= 0)
                {
                    isVisible = false;
                }
            }

            // makes the projectile move and do its thing
            foreach (Projectiles projectile in EnemyPoj)
            {
                projectile.update(Screen, Tiles);

                if (projectile.COLLISION.Intersects(CurrentPlayer.COLLISION) && CurrentPlayer.STATE != PlayerStates.Hit)
                {
                    CurrentPlayer.HitByProjectil(projectile);
                }
            }
            //this makes both the player projectile and the enemy projectile no longer visible when they collide with each other so they can be removed later
            foreach (Projectiles playerProj in CurrentPlayer.PLAYERPROJECTILES)
            {
                for (int P = EnemyPoj.Count - 1; P >= 0; P--)
                    if (EnemyPoj[P].COLLISION.Intersects(playerProj.COLLISION))
                    {
                        EnemyPoj[P].VisibleItIs = false;
                        playerProj.VisibleItIs = false;
                    }
            }
            for (int P = EnemyPoj.Count - 1; P >= 0; P--)
            {
                // removes the projectile when it's no longer visible
                if (EnemyPoj[P].VisibleItIs == false)
                {
                    EnemyPoj.RemoveAt(P);
                    break;
                }


            }

            //this actually moves them
            Pos += (Velocity) * Speed;
            rect.X = (int)Pos.X;
            rect.Y = (int)Pos.Y;
        }
        public override void Draw(SpriteBatch sb, GameTime gt)
        {
            updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * FramesPerSecond;

            if (updateTrigger >= 1)
            {
                updateTrigger = 0;
                sourceRectangle.X += sourceRectangle.Width;
                if (sourceRectangle.X >= texture.Width)
                {
                    // thiw would reset the enemy back to their movement state once they finish their attack
                    if (CurrentAction == EnemyAction.Attack)
                    {
                        CurrentAction = EnemyAction.Movement;
                        sourceRectangle.X = 0;
                    }
                    else if (CurrentAction == EnemyAction.Death)
                    {
                        sourceRectangle.X = texture.Width - sourceRectangle.Width;
                    }
                    else
                    {
                        sourceRectangle.X = 0;
                    }
                }

            }
            //this draws them facing the correct way depending if their flipped or not
            if (!Flipped)
            {
                sb.Draw(texture, rect, sourceRectangle, Color.White * Colour);
            }
            else
            {
                sb.Draw(texture, rect, sourceRectangle, Color.White * Colour, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            }
            //draws the projectiles
            foreach (Projectiles projectile in EnemyPoj)
            {
                projectile.Draw(sb, gt);
            }

#if DEBUG
            sb.Draw(Game1.debugPixel, rect, Color.Peru * 0.5f);

#endif
        }

        //used to stop an issue of the sounds playing when they shouldn't be
        public void STOPSOUND()
        {
            WalkInstance.Stop();
        }

    }
    class MeleeEnemy : Enemies
    {
        private float seektime;

        private readonly Texture2D DEATH;

        public MeleeEnemy(Texture2D txr, Texture2D Death, int X, int Y, float S, int frames, int D, int H, bool flip, SoundEffect WS) : base(txr, X, Y, S, frames, D, H, flip, WS)
        {
            rect = new Rectangle(X, Y, texture.Width / 4, texture.Height / 2);
            sourceRectangle = new Rectangle(0, 0, 86, 96);
            DEATH = Death;
            Damage = (int)(D * 1.25f);
            CurrentAction = EnemyAction.Movement;
            WalkInstance.Pitch = -0.4f;
            WalkInstance.Volume = 0.2f;
        }
        public override void Update(List<CollisionTiles> Tiles, Player CurrentPlayer, Rectangle Screen, GameTime gt, GameState currState)
        {
            base.Update(Tiles, CurrentPlayer, Screen, gt, currState);
            if (isVisible == false || currState == GameState.Paused)
            {
                WalkInstance.Stop();
            }

            switch (CurrentAction)
            {
                case EnemyAction.Movement:
                    sourceRectangle.Y = 0;
                    MeleeMovement(Tiles, CurrentPlayer);
                    // this will reset them back to a state where they weren't searching for the player
                    if (Seeking)
                    {

                        if (!Detection.Intersects(CurrentPlayer.COLLISION))
                        {
                            seektime += (float)gt.ElapsedGameTime.TotalSeconds;
                            if (seektime >= 3)
                            {
                                Seeking = false;
                                seektime = 0;
                            }
                        }
                        else
                        {
                            seektime = 0;
                        }
                    }

                    if (!Flipped) //sets the hit and detection box location
                    {
                        HitRect = new Rectangle((int)Pos.X - 10, (int)Pos.Y, 40, rect.Height);
                        Detection = new Rectangle((int)(Pos.X - 255), (int)(Pos.Y + 5), 310, 35);
                    }
                    else
                    {
                        HitRect = new Rectangle((int)Pos.X + rect.Width / 2 + 10, (int)Pos.Y, 40, rect.Height);
                        Detection = new Rectangle((int)(Pos.X + 35), (int)(Pos.Y + 5), 310, 35);


                    }

                    if (HitRect.Intersects(CurrentPlayer.COLLISION))//switches to the attack if the player intersects with the attack box
                    {
                        CurrentAction = EnemyAction.Attack;
                        sourceRectangle.X = 0;
                    }
                    //this plays the sound if they're on the screen
                    if (rect.Intersects(Screen))
                    {
                        WalkInstance.Play();
                        WalkInstance.Volume = 0.2f;
                    }
                    else
                    {

                        WalkInstance.Stop();

                    }





                    break;

                case EnemyAction.Hit:
                    WalkInstance.Stop();
                    HitRect = Rectangle.Empty;
                    break;
                case EnemyAction.Attack:
                    WalkInstance.Stop();
                    sourceRectangle.Y = sourceRectangle.Height;
                    if (CurrentPlayer.COLLISION.Intersects(HitRect) && CurrentPlayer.STATE != PlayerStates.Hit)
                    {
                        CurrentPlayer.HEALTH -= Damage;
                    }
                    Velocity.X = 0;
                    break;
                case EnemyAction.Death:
                    WalkInstance.Stop();
                    Timer += (float)gt.ElapsedGameTime.TotalSeconds;
                    HitRect = Rectangle.Empty;

                    sourceRectangle.Width = DEATH.Width / 4;
                    sourceRectangle.Height = DEATH.Height;
                    break;
            }
        }
        //this holds all of the melee enemy movement code in one place and makes it easier to alter if it needs to be
        private void MeleeMovement(List<CollisionTiles> Tiles, Player CurrentPlayer)
        {
            //this makes it so it will follow the player when they 
            if (Detection.Intersects(CurrentPlayer.COLLISION))
            {
                Seeking = true;
            }
            if (!Seeking)
            {
                if (Flipped)
                {
                    Velocity.X = 1.5f;
                }
                else
                {
                    Velocity.X = -1.5f;
                }
            }
            else
            {
                if (CurrentPlayer.PlayerPos.X < Pos.X)
                {
                    Velocity.X = -1.5f;
                    Flipped = false;
                }
                else
                {
                    Velocity.X = 1.5f;
                    Flipped = true;
                }

                foreach (CollisionTiles tile in Tiles)
                {
                    if (rect.TouchLeftof(tile.Rectangle))
                    {
                        if (Velocity.X > 0)
                        {
                            Velocity.X = 0;
                        }
                        else
                        {

                        }
                    }
                    if (rect.TouchRightof(tile.Rectangle))
                    {
                        if (Velocity.X < 0)
                        {
                            Velocity.X = 0;
                        }
                        else
                        {

                        }
                    }
                }
            }
            foreach (CollisionTiles tile in Tiles)
            {
                if (rect.TouchRightof(tile.Rectangle) && !Seeking)
                {
                    Flipped = true;
                }
                if (rect.TouchLeftof(tile.Rectangle) && !Seeking)
                {
                    Flipped = false;
                }
            }
        }

        public override void Draw(SpriteBatch sb, GameTime gt)
        {
            // this has a slightly diffrent draw method to everything else but its more to do with messing up the creation of the death asset and will adjust what asset is getting drawn along with the correcft source rectangle
            switch (CurrentAction)
            {
                default:
                    sourceRectangle.Width = texture.Width / 4;

                    base.Draw(sb, gt);
                    break;
                case EnemyAction.Death:
                    sourceRectangle.Width = DEATH.Width / 4;

                    updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * FramesPerSecond;

                    if (updateTrigger >= 1)
                    {
                        updateTrigger = 0;
                        sourceRectangle.X += sourceRectangle.Width;
                        if (sourceRectangle.X >= DEATH.Width)
                        {
                            sourceRectangle.X = sourceRectangle.Width * 3;
                        }

                    }
                    if (!Flipped)
                    {
                        sb.Draw(DEATH, rect, sourceRectangle, Color.White * Colour);
                    }
                    else
                    {
                        sb.Draw(DEATH, rect, sourceRectangle, Color.White * Colour, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
                    }

                    break;
            }
#if DEBUG
            sb.Draw(Game1.debugPixel, rect, Color.Peru * 0.5f);
            sb.Draw(Game1.debugPixel, HitRect, Color.Blue * 0.5f);
            sb.Draw(Game1.debugPixel, Detection, Color.Black * 0.5f);
            sb.DrawString(Game1.debugFont, "Health: " + Health +
                "CurrentAction: " + CurrentAction, Pos, Color.White);

#endif
        }

    }
    class RangeEnemy : Enemies
    {


        public RangeEnemy(Texture2D txr, int X, int Y, float S, int frames, int D, int H, bool flip, SoundEffect impact, Texture2D Arrow) : base(txr, X, Y, S, frames, D, H, flip, impact)
        {
            rect = new Rectangle(X, Y, 48, 96);
            sourceRectangle = new Rectangle(0, 0, txr.Width / 4, txr.Height / 3);
            CurrentAction = EnemyAction.Movement;
            ACTIONSCOUNTER[0] = 0;
            ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 5);
            ProjectileTexture = Arrow;
        }
        public override void Update(List<CollisionTiles> Tiles, Player CurrentPlayer, Rectangle Screen, GameTime gt, GameState currState)
        {
            base.Update(Tiles, CurrentPlayer, Screen, gt, currState);

            if (CurrentPlayer.PlayerPos.X > Pos.X + rect.Width / 2)
            {
                if (CurrentAction != EnemyAction.Death)
                    Flipped = false;
            }
            else
            {
                if (CurrentAction != EnemyAction.Death)
                    Flipped = true;
            }

            switch (CurrentAction)
            {
                //this wont be used for movement by the ranger or the mage but will be used to determine when they will attack;
                case EnemyAction.Movement:
                    sourceRectangle.Y = 0;
                    Timer += (float)gt.ElapsedGameTime.TotalSeconds;
                    if (Timer >= 1)
                    {
                        ACTIONSCOUNTER[0]++;
                        Timer = 0;
                    }

                    if (ACTIONSCOUNTER[0] >= ACTIONSCOUNTER[1])
                    {
                        ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 5);
                        ACTIONSCOUNTER[0] = 0;
                        sourceRectangle.X = 0;
                        Projectile_Count = 0;
                        CurrentAction = EnemyAction.Attack;
                    }
                    break;
                case EnemyAction.Attack:

                    sourceRectangle.Y = texture.Height / 3;
                    if (sourceRectangle.X >= texture.Width - sourceRectangle.Width && Projectile_Count < 1)
                    {
                        if (Flipped)
                        {
                            EnemyPoj.Add(new ArrowProj(ProjectileTexture, (int)Pos.X, (int)Pos.Y + rect.Height / 3, -5, 1, Game1.RNG.Next(8, 24), Tiles, true, ProjWalksound));

                        }
                        else
                        {
                            EnemyPoj.Add(new ArrowProj(ProjectileTexture, (int)Pos.X, (int)Pos.Y + rect.Height / 3, 5, 1, Game1.RNG.Next(8, 24), Tiles, false, ProjWalksound));
                        }
                        CurrentAction = EnemyAction.Movement;
                        Projectile_Count++;
                        //sourceRectangle.Y = 0;
                    }

                    break;
                case EnemyAction.Death:
                    sourceRectangle.Y = sourceRectangle.Height * 2;
                    break;
            }
        }
        public override void Draw(SpriteBatch sb, GameTime gt)
        {

            base.Draw(sb, gt);
#if DEBUG
            sb.DrawString(Game1.debugFont, "Pos: " + Pos +
                "\n Velocity: " + EnemyPoj.Count +
                "\n Current State: " + CurrentAction +
                "\n Flipped: " + Flipped +
                "\n PROJ: " + EnemyPoj.Count, Pos, Color.White);
#endif
        }
    }
    class MageEnemy : Enemies
    {
        private int counter = 0;
        public MageEnemy(Texture2D txr, int X, int Y, float S, int frames, int D, int H, bool flip, SoundEffect WS, Texture2D Bolt) : base(txr, X, Y, S, frames, D, H, flip, WS)
        {
            sourceRectangle.Width = texture.Width / 4;
            sourceRectangle.Height = texture.Height / 3;
            rect.Width = 48;
            rect.Height = 96;
            CurrentAction = EnemyAction.Movement;
            ACTIONSCOUNTER[0] = 0;
            ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 5);
            ProjectileTexture = Bolt;
        }
        public override void Update(List<CollisionTiles> Tiles, Player CurrentPlayer, Rectangle Screen, GameTime gt, GameState currState)
        {
            base.Update(Tiles, CurrentPlayer, Screen, gt, currState);

            if (CurrentPlayer.PlayerPos.X < Pos.X + rect.Width / 2)
            {
                if (CurrentAction != EnemyAction.Death)
                    Flipped = false;
            }
            else
            {
                if (CurrentAction != EnemyAction.Death)
                    Flipped = true;
            }

            //this wont be used for movement by the ranger or the mage but will be used to determine when they will attack;
            switch (CurrentAction)
            {

                case EnemyAction.Movement:
                    sourceRectangle.Y = 0;
                    Timer += (float)gt.ElapsedGameTime.TotalSeconds;
                    if (Timer >= 1)
                    {
                        ACTIONSCOUNTER[0]++;
                        Timer = 0;
                    }

                    if (ACTIONSCOUNTER[0] >= ACTIONSCOUNTER[1])
                    {
                        ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 5);
                        ACTIONSCOUNTER[0] = 0;
                        sourceRectangle.X = 0;
                        counter = 0;
                        CurrentAction = EnemyAction.Attack;
                    }
                    break;
                case EnemyAction.Attack:
                    sourceRectangle.Y = texture.Height / 3;
                    if (sourceRectangle.X >= texture.Width - sourceRectangle.Width && counter == 0)
                    {
                        if (Flipped)
                        {
                            EnemyPoj.Add(new BoltOfLight(ProjectileTexture, (int)Pos.X + rect.Width, (int)Pos.Y + rect.Height / 3, 2, 2, Game1.RNG.Next(8, 24), Tiles, true, ProjWalksound));

                        }
                        else
                        {
                            EnemyPoj.Add(new BoltOfLight(ProjectileTexture, (int)Pos.X, (int)Pos.Y + rect.Height / 3, 2, 2, Game1.RNG.Next(8, 24), Tiles, true, ProjWalksound));

                        }
                        counter++;
                        CurrentAction = EnemyAction.Movement;
                    }
                    break;
                case EnemyAction.Death:
                    sourceRectangle.Y = sourceRectangle.Height * 2;
                    EnemyPoj = new List<Projectiles>();
                    break;
            }
            foreach (Projectiles projectile in EnemyPoj)
            {
                projectile.getTarget(new Vector2(CurrentPlayer.PlayerPos.X + CurrentPlayer.COLLISION.Width / 2, CurrentPlayer.PlayerPos.Y + CurrentPlayer.COLLISION.Height / 2));
                projectile.update(Screen, Tiles);
            }
        }
        public override void Draw(SpriteBatch sb, GameTime gt)
        {
            base.Draw(sb, gt);
#if DEBUG
            sb.DrawString(Game1.debugFont, "Pos: " + Pos +
                "\n Velocity: " + EnemyPoj.Count +
                "\n Current State: " + CurrentAction +
                "\n Flipped: " + Flipped +
                "\n PROJ: " + EnemyPoj.Count, Pos, Color.White);
#endif
        }
    }

    enum BossAction
    {
        Idle,
        Hit,
        Movement,
        Attack1,
        Attack2,
        Attack3,
        Casting,
        Death
    }
    class Boss : Enemies
    {
        // this will used to count up how long it's been since the player went out of range then it will start casting projectiles to get the player to fall from the roof 

        private int maxHealth;
        private int Choice;
        private Texture2D SwordThrownTxr;
        public Boss(Texture2D txr, int X, int Y, float S, int frames, int D, int H, bool flip, SoundEffect WS, Texture2D Projectile, Texture2D Thrown) : base(txr, X, Y, S, frames, D, H, flip, WS)
        {
            SwordThrownTxr = Thrown;
            // this sets the animation draw box
            sourceRectangle.Width = texture.Width / 4;
            sourceRectangle.Height = texture.Height / 5;
            //this sets the actual size of the enemy
            rect.Width = sourceRectangle.Width;
            rect.Height = sourceRectangle.Height;

            //this is going to be part of the enemy condition
            maxHealth = H;

            CurrentMove = BossAction.Idle;
            ACTIONSCOUNTER[0] = 0;
            ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 8);
            Seeking = true;
            ProjectileTexture = Projectile;
        }
        public override void Update(List<CollisionTiles> Tiles, Player CurrentPlayer, Rectangle Screen, GameTime gt, GameState currState)
        {
            if (Health <= 0 && CurrentMove != BossAction.Death)
            {
                Timer = 0;
                Health = 0;
                EnemyPoj.Clear();
                sourceRectangle.X = 0;
                CurrentMove = BossAction.Death;
            }

            //this is the damage of the player when their hit by any of the projectile
            foreach (Projectiles projectile in EnemyPoj)
            {
                projectile.update(Screen, Tiles);

                if (projectile.COLLISION.Intersects(CurrentPlayer.COLLISION) && CurrentPlayer.STATE != PlayerStates.Hit)
                {
                    CurrentPlayer.HEALTH -= projectile.DAMAGE;

                    //projectile.VisibleItIs = false;
                }
            }

            //this means they can take damage when they get hit
            if (CurrentMove == BossAction.Movement || CurrentMove == BossAction.Attack1 || CurrentMove == BossAction.Attack2 || CurrentMove == BossAction.Attack3)
            {
                CanTakeDamage = true;
            }
            else
            {
                //this makes it so if the projectile hits the boss when they're not in a state that they can take damage it will remove the projectile
                foreach (Projectiles playerProj in CurrentPlayer.PLAYERPROJECTILES)
                {
                    if (playerProj.COLLISION.Intersects(rect))
                    {
                        playerProj.VisibleItIs = false;
                    }
                }
                CanTakeDamage = false;
            }


            // this is the vertical gravity contantly getting applied
            Velocity.Y += 1;
            if (Velocity.Y > 12)
            {
                Velocity.Y = 12;
            }
            //this stops the verticle gravity when they touch the top of a tile
            foreach (CollisionTiles tile in Tiles)
            {
                if (rect.TouchTopof(tile.Rectangle))
                {
                    Velocity.Y = 0;
                    Pos.Y = tile.Rectangle.Y - rect.Height;
                }

            }

            //puts thm into the state where the enemy has been hit
            if (CanTakeDamage)
            {

                if (rect.Intersects(CurrentPlayer.HIT))
                {
                    //if (CurrentPlayer.PlayerPos.X < Pos.X)
                    //{
                    //    Velocity.X = 4;
                    //    Flipped = false; // points them towards the player
                    //}
                    //else
                    //{
                    //    Velocity.X = -4;
                    //    Flipped = true; // points them towards the player
                    //}

                    //CanTakeDamage = false;
                    //Velocity.Y = -5;
                    Health -= CurrentPlayer.DAMAGE;
                    //CurrentMove = BossAction.Hit;
                }
                for (int p = 0; p < CurrentPlayer.PLAYERPROJECTILES.Count; p++)
                {
                    if (CurrentPlayer.PLAYERPROJECTILES[p].COLLISION.Intersects(rect) && CurrentPlayer.PLAYERPROJECTILES[p].VisibleItIs)
                    {
                        Health -= CurrentPlayer.PLAYERPROJECTILES[p].DAMAGE;
                        CurrentPlayer.PLAYERPROJECTILES[p].VisibleItIs = false;


                    }
                }
            }
            if (Flipped)// sets where the melee box for the boss is
            {
                HitRect = new Rectangle((int)(Pos.X + rect.Width / 2) + 20, (int)Pos.Y, 40, rect.Height);
            }
            else
            {
                HitRect = new Rectangle((int)(Pos.X) - 20, (int)Pos.Y, 40, rect.Height);

            }
            if (HitRect.Intersects(CurrentPlayer.COLLISION))
            {
                CurrentMove = BossAction.Attack1;
            }

            switch (CurrentMove)
            {
                case BossAction.Idle:
                    sourceRectangle.Y = 0;
                    Timer += (float)gt.ElapsedGameTime.TotalSeconds;
                    if (CurrentPlayer.PlayerPos.X + CurrentPlayer.COLLISION.Width / 2 < Pos.X + rect.Width / 2)
                    {
                        Flipped = false;
                    }
                    else
                    {
                        Flipped = true;
                    }
                    if (Timer > 1)
                    {
                        ACTIONSCOUNTER[0] += (int)Timer;
                        Timer = 0;
                    }
                    // this will determine the action that the boss is gonna do within this section
                    if (ACTIONSCOUNTER[0] >= ACTIONSCOUNTER[1])
                    {
                        Choice = Game1.RNG.Next(0, 3);
                        switch (Choice)
                        {
                            case 0:
                                CurrentMove = BossAction.Movement;
                                sourceRectangle.X = 0;
                                break;
                            case 1:
                                CurrentMove = BossAction.Casting;
                                sourceRectangle.X = 0;
                                break;
                            case 2:
                                CurrentMove = BossAction.Attack2;
                                sourceRectangle.X = 0;
                                break;
                        }
                        Timer = 0;
                        ACTIONSCOUNTER[0] = 0;
                        ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 4);
                        sourceRectangle.X = 0;
                    }
                    break;


                case BossAction.Movement:
                    sourceRectangle.Y = sourceRectangle.Height;
                    foreach (CollisionTiles tile in Tiles)
                    {
                        // this set the speed that the boss will move at depending on what they're facing
                        if (!Flipped && CurrentMove == BossAction.Movement)
                        {
                            if (Health <= maxHealth / 2)
                            {
                                Velocity.X = -11;
                            }
                            else
                            {
                                Velocity.X = -7;

                            }
                        }
                        else if (Flipped && CurrentMove == BossAction.Movement)
                        {
                            if (Health <= maxHealth / 2)
                            {
                                Velocity.X = 11;
                            }
                            else
                            {
                                Velocity.X = 7;

                            }
                        }

                        if (rect.TouchLeftof(tile.Rectangle))
                        {
                            //this will determine what action the enemy will do if it hits a tile 
                            Choice = Game1.RNG.Next(0, 3);
                            Velocity.X = 0;
                            Pos.X = tile.Rectangle.Left - rect.Width - 6;
                            Flipped = false;

                            if (Choice == 0)
                            {
                                CurrentMove = BossAction.Idle;
                                sourceRectangle.X = 0;
                                ACTIONSCOUNTER[0] = 0;
                                ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 3);
                            }
                            if (Choice == 1)
                            {

                                CurrentMove = BossAction.Movement;


                                sourceRectangle.X = 0;

                            }
                            if (Choice == 2)
                            {
                                CurrentMove = BossAction.Attack2;
                                sourceRectangle.X = 0;
                                Projectile_Count = 0;

                            }
                        }
                        else if (rect.TouchRightof(tile.Rectangle))
                        {
                            //this will determine what action the enemy will do if it hits a tile 
                            int Choice = Game1.RNG.Next(0, 4);
                            Velocity.X = 0;
                            Pos.X += 5;
                            Flipped = true;

                            if (Choice == 0)
                            {
                                CurrentMove = BossAction.Idle;
                                sourceRectangle.X = 0;
                                ACTIONSCOUNTER[0] = 0;
                                ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 3);
                            }
                            if (Choice == 1)
                            {
                                CurrentMove = BossAction.Movement;
                                sourceRectangle.X = 0;

                            }
                            if (Choice == 2)
                            {
                                CurrentMove = BossAction.Attack2;
                                sourceRectangle.X = 0;
                                Projectile_Count = 0;
                            }
                        }

                    }
                    if (HitRect.Intersects(CurrentPlayer.COLLISION))
                    {
                        sourceRectangle.X = 0;
                        CurrentMove = BossAction.Attack1;
                    }
                    break;
                case BossAction.Casting:
                    // this is the pre requisit to the raining blades attack
                    sourceRectangle.Y = sourceRectangle.Height * 3;
                    Projectile_Count = 0;
                    if (sourceRectangle.X >= texture.Width - sourceRectangle.Width)
                    {
                        CurrentMove = BossAction.Attack3;

                    }
                    break;
                case BossAction.Attack1:
                    Velocity.X = 0;
                    sourceRectangle.Y = sourceRectangle.Height * 2;
                    // this will do the melee attack when it hits this condition
                    if (sourceRectangle.X >= texture.Width - sourceRectangle.Width)
                    {
                        Timer += (float)gt.ElapsedGameTime.TotalSeconds;
                        if (CurrentPlayer.HasBeenHit)
                        {
                            Timer = 0;

                            CurrentMove = BossAction.Movement;
                            if (Flipped)
                            {
                                Flipped = false;
                            }
                            else
                            {
                                Flipped = true;
                            }
                            sourceRectangle.X = 0;
                            ACTIONSCOUNTER[0] = 0;
                            ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 3);
                        }
                        if (Timer > 1.25) // will move away from the player once the timer meets this condition
                        {
                            Timer = 0;

                            CurrentMove = BossAction.Movement;
                            if (Flipped)
                            {
                                Flipped = false;
                            }
                            else
                            {
                                Flipped = true;
                            }
                            sourceRectangle.X = 0;
                            ACTIONSCOUNTER[0] = 0;
                            ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 3);
                        }

                    }
                    break;
                case BossAction.Attack2:
                    sourceRectangle.Y = sourceRectangle.Height * 2;
                    // this will launch a projectile towards the player when it gets to this state
                    if (sourceRectangle.X >= texture.Width - sourceRectangle.Width)
                    {
                        if (Flipped)
                        {
                            if (Projectile_Count < 1)
                            {
                                EnemyPoj.Add(new SwordThrown(SwordThrownTxr, (int)(Pos.X + rect.Width), (int)(Pos.Y + rect.Height / 2 - SwordThrownTxr.Height / 2), 3, 1, Game1.RNG.Next(15, 30), Tiles, Flipped, ProjWalksound));
                                Projectile_Count++;
                                break;
                            }
                        }
                        else
                        {
                            if (Projectile_Count < 1)
                            {
                                EnemyPoj.Add(new SwordThrown(SwordThrownTxr, (int)(Pos.X - SwordThrownTxr.Width), (int)(Pos.Y + rect.Height / 2 - SwordThrownTxr.Height / 2), -3, 1, Game1.RNG.Next(15, 30), Tiles, Flipped, ProjWalksound));
                                Projectile_Count++;
                                break;
                            }
                        }
                        Timer += (float)gt.ElapsedGameTime.TotalSeconds;
                        // will randomly choose an action to go into once it hits this timer
                        if (Timer > 3f)
                        {
                            Projectile_Count = 0;
                            Choice = Game1.RNG.Next(0, 3);
                            switch (Choice)
                            {
                                case 0:
                                    CurrentMove = BossAction.Idle;
                                    sourceRectangle.X = 0;
                                    ACTIONSCOUNTER[0] = 0;
                                    ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 3);
                                    break;
                                case 1:
                                    CurrentMove = BossAction.Movement;
                                    sourceRectangle.X = 0;
                                    break;
                                case 2:
                                    CurrentMove = BossAction.Casting;
                                    sourceRectangle.X = 0;
                                    Projectile_Count = 0;
                                    break;


                            }
                        }

                    }

                    break;

                case BossAction.Attack3:
                    // this creates projectiles that will fall towards the player till it hits the counter
                    if (Projectile_Count < 60)
                    {
                        Timer += (float)gt.ElapsedGameTime.TotalSeconds;
                        if (Timer > 0.1)
                        {
                            EnemyPoj.Add(new Swords(ProjectileTexture, Game1.RNG.Next(0, Screen.Width - 100), 0 - ProjectileTexture.Height, Game1.RNG.Next(2, 4), 1, Game1.RNG.Next(5, 15), Tiles, false, ProjWalksound));
                            Projectile_Count++;
                            Timer = 0;
                        }
                    }
                    else
                    {
                        CurrentMove = BossAction.Idle;
                        ACTIONSCOUNTER[0] = 0;
                        ACTIONSCOUNTER[1] = Game1.RNG.Next(1, 3);
                        Projectile_Count = 0;
                    }

                    break;

                case BossAction.Death:
                    sourceRectangle.Y = sourceRectangle.Height * 4;
                    HitRect = Rectangle.Empty;
                    Velocity.X = 0;

                    Timer += (float)gt.ElapsedGameTime.TotalSeconds;

                    if (Timer > 5f) // makes them begin to fade
                    {
                        Colour -= 0.02f;

                    }

                    if (Colour <= 0f)
                    {
                        isVisible = false;
                    }
                    break;
                    // although this isn't atually used after a change made i don't want to risk breaking it by cutting it out incase this is called for whatever reason even though it shouldn't be
                case BossAction.Hit:
                    sourceRectangle.X = 0;
                    sourceRectangle.Y = 0;
                    HitRect = Rectangle.Empty;

                    CanTakeDamage = false;
                    // this slows them down so they can revert back to their previous state
                    if (Velocity.X > 0)
                    {
                        Velocity.X -= (float)gt.ElapsedGameTime.TotalSeconds * 3;
                    }
                    else
                    {
                        Velocity.X += (float)gt.ElapsedGameTime.TotalSeconds * 3;
                    }
                    if ((int)Velocity.X == 0)//reverts them back to being idle
                    {
                        Velocity.X = 0;
                        CurrentMove = BossAction.Idle;
                        ACTIONSCOUNTER[0] = 0;
                        ACTIONSCOUNTER[1] = Game1.RNG.Next(0, 3);
                    }
                    foreach (CollisionTiles tile in Tiles)
                    {
                        if (rect.TouchLeftof(tile.Rectangle))
                        {
                            if (Velocity.X > 0)
                            {
                                Velocity.X = 0;
                                Pos.X = tile.Rectangle.X - sourceRectangle.Width - 5;
                            }
                            else
                            {

                            }
                        }
                        if (rect.TouchRightof(tile.Rectangle))
                        {
                            if (Velocity.X < 0)
                            {
                                Velocity.X = 0;
                                Pos.X = tile.Rectangle.X + tile.Rectangle.Width + 5;

                            }
                            else
                            {

                            }
                        }
                    }
                    break;

            }

            //makes the enemies projectile move and do its thing 
            foreach (Projectiles projectile in EnemyPoj)
            {
                projectile.update(Screen, Tiles);
            }

            //this will remove a projectile if it collides with one of the players projectiles
            foreach (Projectiles playerProj in CurrentPlayer.PLAYERPROJECTILES)
            {
                for (int P = EnemyPoj.Count - 1; P >= 0; P--)
                    if (EnemyPoj[P].COLLISION.Intersects(playerProj.COLLISION))
                    {
                        EnemyPoj[P].VisibleItIs = false;
                        playerProj.VisibleItIs = false;
                    }
            }
            for (int P = EnemyPoj.Count - 1; P >= 0; P--)
            {

                if (EnemyPoj[P].VisibleItIs == false)
                {
                    EnemyPoj.RemoveAt(P);
                    break;
                }


            }

            //this moves them on the screen
            Pos += (Velocity) * Speed;
            rect.X = (int)Pos.X;
            rect.Y = (int)Pos.Y;

        }
        public override void Draw(SpriteBatch sb, GameTime gt)
        {
            switch (CurrentMove)
            {
                case BossAction.Idle:
                    Drawing(sb, gt, 6);


                    break;
                case BossAction.Movement:
                    Drawing(sb, gt, 8);

                    break;
                case BossAction.Casting:
                    Drawing(sb, gt, 4);

                    break;
                case BossAction.Attack1:
                    Drawing(sb, gt, 10);

                    break;
                case BossAction.Attack2:
                    Drawing(sb, gt, 8);

                    break;

                case BossAction.Attack3:
                    Drawing(sb, gt, 6);

                    break;
                case BossAction.Death:
                    Drawing(sb, gt, 6);

                    break;
                case BossAction.Hit:
                    Drawing(sb, gt, 0);

                    break;
            }
            foreach (Projectiles projectile in EnemyPoj)
            {
                projectile.Draw(sb, gt);
            }
#if DEBUG
            sb.DrawString(Game1.debugFont, "Pos: " + Pos +
                "\nVelocity: " + Velocity +
                "\nCan Take Damage: " + CanTakeDamage +
                "\nFlipped: " + Flipped +
                "\nHealth: " + Health +
                "\nCurrent Action: " + CurrentMove +
                "\nTimer: " + Timer +
                "\nProjectile Counter: " + Projectile_Count +
                "\nActual Projectiles: " + EnemyPoj.Count, Pos, Color.White);
            sb.Draw(Game1.debugPixel, rect, Color.Red * 0.5f);
            sb.Draw(Game1.debugPixel, HitRect, Color.Purple * 0.5f);

#endif
        }
        /// <summary>
        /// 
        ///             both this and the drawing method work identically as the one within the player update 
        ///             
        ///             only diffrence is that this wont draw a vertical flip of the asset
        /// </summary>
        /// <param name="Frames"></param>
        /// <param name="gt"></param>
        void UpdateTrigger(int Frames, GameTime gt)
        {
            updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * Frames;

            if (updateTrigger >= 1)
            {
                updateTrigger = 0;
                sourceRectangle.X += sourceRectangle.Width;
                if (sourceRectangle.X >= texture.Width)// this stops the animation at the end of it
                {
                    if (CurrentMove == BossAction.Casting || CurrentMove == BossAction.Attack3 || CurrentMove == BossAction.Attack1 || CurrentMove == BossAction.Attack2 || CurrentMove == BossAction.Death)
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
        void Drawing(SpriteBatch sb, GameTime gt, int Frames)
        {
            // this is for drawing the player for when they are moving right
            UpdateTrigger(Frames, gt);
            if (!Flipped)
            {
                sb.Draw(texture, rect, sourceRectangle, Color.White * Colour);
            }
            else
            {
                sb.Draw(texture, rect, sourceRectangle, Color.White * Colour, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            }
        }

        /// <summary>
        /// 
        ///         this draws the boss health bar when they are in the game
        ///         works similar to the players health bar and will scale in size
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="graphics"></param>
        /// <param name="HealthBar"></param>
        /// <param name="OuterBar"></param>
        /// <param name="BarEndCap"></param>
        /// <param name="HudFont"></param>
        public void DrawHealth(SpriteBatch sb, GraphicsDeviceManager graphics, Texture2D HealthBar, Texture2D OuterBar, Texture2D BarEndCap, SpriteFont HudFont)
        {


            sb.Draw(BarEndCap, new Rectangle(20, graphics.PreferredBackBufferHeight - BarEndCap.Height - 10, BarEndCap.Width, BarEndCap.Height), Color.White);
            sb.Draw(OuterBar, new Rectangle(20 + BarEndCap.Width, graphics.PreferredBackBufferHeight - BarEndCap.Height - 10, maxHealth, OuterBar.Height), Color.White);
            if (!CanTakeDamage)
            {
                sb.Draw(HealthBar, new Rectangle(23, graphics.PreferredBackBufferHeight - HealthBar.Height - 16, Health, HealthBar.Height), Color.Blue);
            }
            else
            {
                sb.Draw(HealthBar, new Rectangle(23, graphics.PreferredBackBufferHeight - HealthBar.Height - 16, Health, HealthBar.Height), Color.Red);

            }
            sb.Draw(BarEndCap, new Rectangle(20 + maxHealth, graphics.PreferredBackBufferHeight - BarEndCap.Height - 10, BarEndCap.Width, BarEndCap.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);

            sb.DrawString(HudFont, Health + " / " + maxHealth, new Vector2(40, graphics.PreferredBackBufferHeight - BarEndCap.Height), Color.White);

        }
    }
}
