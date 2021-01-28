using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace TwistedSoul
{
    class Projectiles : AnimatedGraphics
    {
        protected List<CollisionTiles> Tiles;
        protected Vector2 Targ;
        protected string TypeofProj;
        protected SoundEffect ProjectileSound;
        public String TYPEPROJ
        {
            get
            {
                return TypeofProj;
            }

        }

        protected bool Targetobtained;

        public bool OBTAINEDTARG 
        {
            get 
            {
                return Targetobtained;
            }
            set 
            {
                Targetobtained = value;
            }
        }

        public Projectiles(Texture2D txr, int X, int Y, float S, int frames, int D, List<CollisionTiles> TILES, bool Flipped,SoundEffect ProjSound) : base(txr, X, Y, S, frames, D, Flipped)
        {
            isVisible = true;
            Tiles = TILES;
            ProjectileSound = ProjSound;
        }
        public virtual void update(Rectangle Screen, List<CollisionTiles> Tiles)
        {
            if (!rect.Intersects(Screen))
            {
                isVisible = false;
            }
            foreach (CollisionTiles tile in Tiles)
            {
                if (rect.Intersects(tile.Rectangle))
                {
                    isVisible = false;
                }
            }
        }
        public void getTarget(Vector2 Target)
        {
            Targ = Target;
        }
        public override void Draw(SpriteBatch sb, GameTime gt)
        {
            base.Draw(sb, gt);
        }
    }

    /// <summary>
    ///         This will be the arrow class that the range enemies will be able to create
    ///         The player will be able to create these but only if they counter it and depending what variation of the counter will depend on the damage it does and the speed it moves at.
    /// </summary>

    class ArrowProj : Projectiles
    {
        
        public ArrowProj(Texture2D txr, int X, int Y, float S, int frames, int D, List<CollisionTiles> TILES, bool FLIPPED,SoundEffect ProjSound) : base(txr, X, Y, S, frames, D, TILES, FLIPPED,ProjSound)
        {
            rect.Width = (rect.Width / 2);
            rect.Height = (rect.Height / 2);
            TypeofProj = "arrow";
            //ProjectileSound.Play();
          

        }

        public override void update(Rectangle Screen, List<CollisionTiles> Tiles)
        {
            foreach(CollisionTiles tile in Tiles) 
            {
                if (rect.Intersects(tile.Rectangle)) 
                {
                    ProjectileSound.Play();
                } 
            
            }
            
            base.update(Screen, Tiles);
            Velocity.X = 4;
            Pos += (Velocity * Speed);
            rect.X = (int)Pos.X;
        }

    }

    /// <summary>
    ///         This will be the bat projectile that the player will be able to produce.
    ///         This will be created when the player casts it and they meet the requirements
    /// </summary>
    class BatProj : Projectiles
    {
        private SoundEffectInstance ProjInstance;
        public BatProj(Texture2D txr, int X, int Y, float S, int frames, int D, List<CollisionTiles> TILES, bool flip,SoundEffect Sound) : base(txr, X, Y, S, frames, D, TILES, flip,Sound)
        {
            rect = new Rectangle(X, Y, texture.Width / 4, texture.Height);
            sourceRectangle = new Rectangle(0, 0, texture.Width / 4, texture.Height);
            TypeofProj = "bat";

            //this plays the flapping soundeffect 
            ProjInstance = Sound.CreateInstance();
            ProjInstance.Volume = 0.5f;
            ProjInstance.IsLooped = true;
            ProjInstance.Play();
        }
        public override void update(Rectangle Screen, List<CollisionTiles> Tiles)
        {
            base.update(Screen, Tiles);
            if (!Flipped)
            {
                Velocity.X = 3;
            }
            else
            {
                Velocity.X = -3;
            }
            if(isVisible == false) 
            {
                ProjInstance.Stop();
            }


            Pos += (Velocity * Speed);
            rect.X = (int)Pos.X;
        }

    }

    /// <summary>
    ///         This will be the projectile that the mage enemy will be able to create to attack the enemy        
    ///         This will be another instance of where the player can launch it back at the enemy if they use it and depending on the counter depends on the damage output again
    ///         
    /// </summary>
    class BoltOfLight : Projectiles
    {
        private double rotation;
        public BoltOfLight(Texture2D txr, int X, int Y, float S, int frames, int D, List<CollisionTiles> TILES, bool flip,SoundEffect sound) : base(txr, X, Y, S, frames, D, TILES, flip,sound)
        {
            sourceRectangle.Width = texture.Width / 2;
            sourceRectangle.Height = texture.Height;
            rect.Width /= 2;
            rect.Height /= 2;
            TypeofProj = "bolt";

        }
        public override void update(Rectangle Screen, List<CollisionTiles> Tiles)
        {
            base.update(Screen, Tiles);
            if (isVisible == false)
            {
                if (rect.Intersects(Screen))
                    ProjectileSound.Play();
            }

            if (Targ.X < Pos.X)
            {
                Velocity.X -= 0.02f;
                if (Velocity.X < -3f)
                {
                    Velocity.X = -3f;
                }
            }
            else
            {
                Velocity.X += 0.02f;
                if (Velocity.X > 3f)
                {
                    Velocity.X = 3f;
                }
            }
            if (Targ.Y < Pos.Y)
            {
                Velocity.Y -= 0.004f;
                if (Velocity.Y < -3f)
                {
                    Velocity.Y = -3f;
                }
            }
            else
            {
                Velocity.Y += 0.004f;
                if (Velocity.Y > 3f)
                {
                    Velocity.Y = 3f;
                }
            }
            if (Targ.Y == Pos.Y)
            {
                Velocity.Y = 0;
            }
            rotation = Math.Atan2((Targ.Y - Pos.Y), (Targ.X - Pos.X));



            Pos += Velocity * Speed;

            rect.X = (int)Pos.X - rect.Width/2;
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
                    sourceRectangle.X = 0;
                }

            }
            sb.Draw(texture, Pos, sourceRectangle, Color.White,(float)rotation, new Vector2(rect.Width / 2, rect.Height / 2), 1f,SpriteEffects.None,1f);
            //sb.Draw(texture, Pos, sourceRectangle, Color.White, (float)rotation, new Vector2(rect.Width / 2, rect.Height / 2), SpriteEffects.None, 0f);
#if DEBUG
            sb.Draw(Game1.debugPixel, rect, Color.Blue * 0.5f);
#endif
        }
    }

    /// <summary>
    ///         This will be the knife projectile that the player can throw.
    /// </summary>
    class KnifeProj : Projectiles
    {
        public KnifeProj(Texture2D txr, int X, int Y, float S, int frames, int D, List<CollisionTiles> TILES, bool flip,SoundEffect sound) : base(txr, X, Y, S, frames, D, TILES, flip,sound)
        {
            rect = new Rectangle(X, Y, texture.Width / 2, texture.Height / 2);
            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            TypeofProj = "knife";

        }
        public override void update(Rectangle Screen, List<CollisionTiles> Tiles)
        {
            base.update(Screen, Tiles);
            Velocity.X = 2;
            Pos += (Velocity * Speed);
            rect.X = (int)Pos.X;

            if (isVisible == false)
            {
                ProjectileSound.Play();
            }
        }

    }
    class SwordThrown : Projectiles 
    {
        public SwordThrown(Texture2D txr, int X, int Y, float S, int frames, int D, List<CollisionTiles> TILES, bool flip,SoundEffect Sound) : base(txr, X, Y, S, frames, D, TILES, flip,Sound)
        {
            rect = new Rectangle(X, Y, texture.Width, texture.Height);
            sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
            TypeofProj = "sword";

        }
        public override void update(Rectangle Screen, List<CollisionTiles> Tiles)
        {
            base.update(Screen, Tiles);
            Velocity.X = 2;
            Pos += (Velocity * Speed);
            rect.X = (int)Pos.X;
            if (isVisible == false)
            {
                ProjectileSound.Play();
            }
        }
    }
    /// <summary>
    ///         Although not a projectile this will be treated as one to keep anything that the player can spawn in one place
    ///         and for the most part will behave the same as the arrow
    ///         this will also be used for the throwing of the sword by the boss but will counter with the knife texture instead
    /// </summary>
    class SkeletalGrasp : Projectiles
    {
        private bool Reverse;
        private float timer;
        private int seconds;

        public SkeletalGrasp(Texture2D txr, int X, int Y, float S, int frames, int D, List<CollisionTiles> TILES, bool flip,SoundEffect sound) : base(txr, X, Y, S, frames, D, TILES, flip,sound)
        {
            rect = new Rectangle(X, Y, texture.Width / 8, texture.Height / 2);
            sourceRectangle = new Rectangle(0, 0, texture.Width / 4, texture.Height);
        }
        public override void Draw(SpriteBatch sb, GameTime gt)
        {
            updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * FramesPerSecond;

            if (updateTrigger >= 1)
            {
                updateTrigger = 0;
                if (!Reverse)
                {
                    if (sourceRectangle.X != sourceRectangle.Width * 3)
                        sourceRectangle.X += sourceRectangle.Width;
                }
                else
                {
                    sourceRectangle.X -= sourceRectangle.Width;
                }
                if (sourceRectangle.X >= texture.Width)
                {
                    sourceRectangle.X = sourceRectangle.Width * 3;
                }

            }
            if (!Reverse && sourceRectangle.X == sourceRectangle.Width * 3)
            {
                timer += (float)gt.ElapsedGameTime.TotalSeconds;
                if (timer >= 1f)
                {
                    seconds += (int)timer;
                    timer = 0f;
                }
                if (seconds >= 2)
                {
                    Reverse = true;
                }
            }
            if (Reverse && sourceRectangle.X == 0)
            {
                isVisible = false;
            }
            if (Flipped == false)
            {
                sb.Draw(texture, rect, sourceRectangle, Color.White);
            }
            else
            {
                sb.Draw(texture, rect, sourceRectangle, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipVertically, 0f);
            }
#if DEBUG
            sb.Draw(Game1.debugPixel, rect, Color.DarkBlue * 0.5f);
#endif
        }
    }
    /// <summary>
    ///         This will be the projectiles that the Boss Will Spawn
    ///         THESE WILL NOT BE COUNTERED BACK TO THE BOSS BUT CAN BE BLOCKED
    /// </summary>
    class Swords : Projectiles
    {
        public Swords(Texture2D txr, int X, int Y, float S, int frames, int D, List<CollisionTiles> TILES, bool flip,SoundEffect sound) : base(txr, X, Y, S, frames, D, TILES, flip,sound)
        {
            sourceRectangle.Width = texture.Width;
            sourceRectangle.Height = texture.Height;
            Velocity.Y = Game1.RNG.Next(2, 6);

            Damage = D;

            rect.Size = sourceRectangle.Size; 
        }
        public override void update(Rectangle Screen, List<CollisionTiles> Tiles)
        {
            
            if(Pos.Y > Screen.Bottom) 
            {
                isVisible = false;
            }
            if (isVisible == false)
            {
             
            }
            Pos += Velocity * Speed;

            rect.Location = new Point((int)Pos.X, (int)Pos.Y);
        }

    }
    
}
