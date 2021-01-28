using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TwistedSoul
{
    /// <summary>
    /// this is a blueprint for others to inherit from so it can be ignored for the most part as its fairly straight forward stuff in this section of the game
    /// </summary>
    abstract class StaticGraphic
    {
        protected Texture2D texture;
        protected Rectangle rect;
        public Rectangle COLLISION
        {
            get
            {
                return rect;
            }
        }
        protected Vector2 Pos;
        public StaticGraphic(Texture2D txr, int X, int Y)
        {
            texture = txr;
            Pos = new Vector2(X, Y);
            rect = new Rectangle(X, Y, texture.Width, texture.Height);

        }
        public virtual void Update()
        {

        }
        public virtual void Draw(SpriteBatch sb, GameTime gt)
        {
            sb.Draw(texture, rect, Color.White);

#if DEBUG
            //input Debug stuff here
            sb.Draw(Game1.debugPixel, rect, Color.Blue * 0.5f);
            sb.DrawString(Game1.debugFont, "Pos: " + Pos + "\nRect: " + rect, Pos, Color.White);

#endif
        }

    }

    /// <summary>
    ///         same idea as the static graphic and this one inherits directly from that and builds up a bit more to the complexity of it but still mairly simple stuff
    /// </summary>
    abstract class MotionGraphics : StaticGraphic
    {
        protected Vector2 Velocity;
        protected float Speed;

        public MotionGraphics(Texture2D txr, int X, int Y, float S) : base(txr, X, Y)
        {
            Velocity = Vector2.Zero;
            Speed = S;
        }
        public override void Update()
        {
            Pos += Velocity * Speed;
        }
        public override void Draw(SpriteBatch sb, GameTime gt)
        {
            rect.X = (int)Pos.X;
            rect.Y = (int)Pos.Y;

            base.Draw(sb, gt);
        }
    }
    /// <summary>
    ///     again same idea as motion graphics but adds most of the final relevant details that will be required for the player or enemies 
    /// </summary>
    abstract class AnimatedGraphics : MotionGraphics
    {
        protected int Damage;
        public int DAMAGE
        {
            get
            {
                return Damage;
            }
        }
        protected int FramesPerSecond;
        protected float updateTrigger;
        protected Rectangle sourceRectangle, DamageBox;
        protected bool Flipped;
        protected bool isVisible;
        public bool VisibleItIs
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

        public AnimatedGraphics(Texture2D txr, int X, int Y, float S, int frames, int D, bool FF) : base(txr, X, Y, S)
        {
            Damage = D;
            FramesPerSecond = frames;
            sourceRectangle = new Rectangle(0, 0, rect.Width / frames, rect.Height);
            Flipped = FF;
        }
        public override void Update()
        {

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
                    sourceRectangle.X = 0;
                }

            }
            if (Flipped == false)
            {
                sb.Draw(texture, rect, sourceRectangle, Color.White);
            }
            else
            {
                sb.Draw(texture, rect, sourceRectangle, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            }

#if DEBUG
            // insert debug stuff here
            sb.Draw(Game1.debugPixel, rect, Color.Blue * 0.5f);
            sb.DrawString(Game1.debugFont, "Pos: " + Pos + "\nRect: " + rect, Pos, Color.White);
#endif
        }
    }


    /// <summary>
    ///         this inherits from the motion graphics class and its whole purpose is to move a source graphic to give the illusion that the background is moving when it is not
    /// </summary>

    sealed class backgrounds : MotionGraphics
    {
        private Rectangle Source;


        public backgrounds(Texture2D txr, int Xpos, int Ypos, int ScreenWidth, int ScreenHeight, float SS) : base(txr, Xpos, Ypos, SS)
        {

            rect = new Rectangle((int)Pos.X, (int)Pos.Y, ScreenWidth, ScreenHeight);//this is only going to be used for drawing to the screen bounds and getting the Width for a single if statement
            Source = rect;
        }
        public void Update(Player currentPlayer, LEVEL CurrentLevel)
        {
            if (CurrentLevel != LEVEL.Boss1 && CurrentLevel != LEVEL.TestArea && CurrentLevel != LEVEL.Level2 && CurrentLevel != LEVEL.Level4)
            {

                if (currentPlayer.PlayerPos.X < rect.Width / 2)
                {

                }
                else
                {
                    Velocity.X = currentPlayer.BackgroundVelocity.X;

                    Pos += Velocity * Speed;

                }
            }
        }

        /// <summary>
        /// this is used on the start screen,load screen or controls screen make the background slowly move in the menu
        /// </summary>
        public void MenuUpdate()
        {
            Velocity.X = 1;

            Pos += Velocity * Speed;
        }
        public override void Draw(SpriteBatch sb, GameTime gt)
        {
            Source.X = (int)Pos.X;

            sb.Draw(texture, rect, Source, Color.White);

#if DEBUG
            sb.DrawString(Game1.debugFont, "Source: " + Source, new Vector2(500, 500), Color.White);
#endif
        }

    }

    /// <summary>
    /// 
    ///         this is used to create particles around the player or enemies upon certain condtions being met such as the enemy dying or the player healing
    ///         
    ///         this doesn't inherit from anything but is sealed so nothing can be a child to this class
    /// 
    /// </summary>
    sealed class Particle
    {
        private static ContentManager content;
        public static ContentManager Content
        {
            private get { return content; }
            set { content = value; }
        }
        private Color ParticleColor;
        private Vector2 ParticlePosition, Velocity;
        public Vector2 POS
        {
            get
            {
                return ParticlePosition;
            }
        }



        private float SPEED, Timer, transparency;
        private int Seconds = 0;
        private bool isVisible;
        public bool VisibleItIs
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


        public Particle(int Xpos, int Ypos, float speed)
        {
            ParticleColor = new Color(Game1.RNG.Next(0, 255), Game1.RNG.Next(0, 255), Game1.RNG.Next(0, 255));
            Game1.RNG.Next(0, 100);
            ParticlePosition = new Vector2(Xpos, Ypos);
            Velocity = new Vector2(Game1.RNG.Next(-2, 3), Game1.RNG.Next(-2, 0));
            SPEED = speed;
            transparency = 1f;
            isVisible = true;

        }

        public void Update(GameTime gt)
        {
            //ParticleColor = new Color(Game1.RNG.Next(0, 255), Game1.RNG.Next(0, 255), Game1.RNG.Next(0, 255));

            if (Velocity.X > 0)
            {
                Velocity.X -= (float)gt.ElapsedGameTime.TotalSeconds * SPEED;
            }
            else
            {
                Velocity.X += (float)gt.ElapsedGameTime.TotalSeconds * SPEED;
            }

            Velocity.Y += (float)gt.ElapsedGameTime.TotalSeconds * SPEED / 2;

            if (Velocity.Y > 1)
            {
                Velocity.Y = 1;
            }
            ParticlePosition += Velocity;

            Timer += (float)gt.ElapsedGameTime.TotalSeconds;
            if (Timer > 1f)
            {
                ParticleColor = new Color(Game1.RNG.Next(0, 255), Game1.RNG.Next(0, 255), Game1.RNG.Next(0, 255));
                Seconds += (int)Timer;
                Timer = 0f;
            }
            if (Seconds >= 3)
            {
                transparency -= 0.02f;
            }
            if (transparency < 0f)
            {
                isVisible = false;
            }


        }

        public void Drawing(SpriteBatch sb)
        {
            sb.Draw(Content.Load<Texture2D>("Particle"), new Rectangle(new Point((int)ParticlePosition.X, (int)ParticlePosition.Y), new Point(3, 3)), ParticleColor * transparency);
#if DEBUG
            // sb.DrawString(Game1.debugFont, "Velocity: " + Velocity, ParticlePosition, Color.Violet);
#endif
        }
    }
}
