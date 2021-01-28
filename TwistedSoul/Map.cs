using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace TwistedSoul
{

    [Serializable]
    public struct MapData
    {
        public int[] tile;


        public MapData(int[,] count)
        {
            tile = new int[count.GetLength(0) * count.GetLength(1)];


        }

    }

    class Tile
    {
        protected Texture2D texture;

        private Rectangle rectangle;
        public Rectangle Rectangle
        {
            get { return rectangle; }
            protected set { rectangle = value; }
        }
        private static ContentManager content;
        public static ContentManager Content
        {
            protected get { return content; }
            set { content = value; }
        }

        protected bool IMPASSABLE;

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rectangle, Color.White);
#if DEBUG
            spriteBatch.Draw(Game1.debugPixel, rectangle, Color.Red * 0.5f);
#endif
        }
    }
    class CollisionTiles : Tile
    {
        private int ii;
        public int II
        {
            get
            {
                return ii;
            }
        }

        public CollisionTiles(int i, Rectangle newRectangle)
        {
            ii = i;
            texture = Content.Load<Texture2D>("Tiles/" + i);
            this.Rectangle = newRectangle;

            if (ii >= 1)
            {
                IMPASSABLE = true;
                //Console.WriteLine(IMPASSABLE);
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

#if DEBUG
            spriteBatch.DrawString(Game1.debugFont, "Value " + ii + "\nPosX " + Rectangle.X + "\nPosY " + Rectangle.Y, new Vector2(Rectangle.X, Rectangle.Y), Color.Yellow);

#endif
        }
    }
    sealed class Map
    {
        private List<CollisionTiles> collisionTiles = new List<CollisionTiles>();

        private MapData _data;

        public List<CollisionTiles> CollisionTiles
        {
            get { return collisionTiles; }
        }

        private int width, height;

        public int Width
        {
            get { return width; }
        }
        public int Height
        {
            get { return height; }
        }

        public Map()
        {
        }

        public void Generate(int Width, int Height, int size, string Filename)
        {


            LoadLevel(Filename);
            // j reprents what point its on for the verticle axis whilst X is for the horizontal axis for the map
            int j = 0;
            while (j < Height)
            {

                for (int i = 0; i < Width; i++)
                {

                    int number = _data.tile[i + (Width * j)]; // this is like this as the list of number in the file that is loaded in is a singular array of ints
                    if (number > 0)
                    {
                        collisionTiles.Add(new CollisionTiles(number, new Rectangle(i * size, j * size, size, size)));
                    }
                    if (i == Width - 1)
                    {
                        j++;
                    }
                    width = (i + 1) * size;

                    height = (j + 1) * size;
                }
            }


        }



        private void LoadLevel(string FileName)
        {
            // this is used to load the correct map into the game
            FileStream stream;

            try
            {
                // Open the file - but read only mode!

                stream = File.Open("Content/Levels/" + FileName, FileMode.OpenOrCreate, FileAccess.Read);
                // Read the data from the file
                XmlSerializer serializer = new XmlSerializer(typeof(MapData));
                _data = (MapData)serializer.Deserialize(stream);
                Debug.WriteLine("IT HAS LOADED THE LEVEL");
            }
            catch (Exception error) // The code in "catch" is what happens if the "try" fails.
            {
                Debug.WriteLine("Load has failed because of: " + error.Message);
            }
        }
        public void ArenaGenerate(int[] BossTiles, int size)
        {
            // boss tiles is essentially all the tiles to fill the screen and the code bellow makes sure they have the right value for all the tiles in the map

            for (int x = 0; x < BossTiles[0]; x++)
                for (int y = 0; y < BossTiles[1]; y++)
                {
                    int number = 0;
                    if (x == 0 && y == 0)
                    {
                        number = 2;
                    }
                    if (y == 0)
                    {
                        if (x > 0 && x < BossTiles[0])
                        {
                            number = 4;
                        }
                        if (x == BossTiles[0] - 1)
                        {
                            number = 3;
                        }
                    }
                    if (y > 0 && y < BossTiles[1] - 1)
                    {
                        if (x == 0)
                        {
                            number = 7;
                        }
                        else if (x == BossTiles[0] - 1)
                        {
                            number = 6;
                        }
                        else
                        {
                            number = 0;
                        }
                    }
                    if (y == BossTiles[1] - 1)
                    {
                        if (x == 0)
                        {
                            number = 8;
                        }
                        else if (x == BossTiles[0] - 1)
                        {
                            number = 9;
                        }
                        else
                        {
                            number = 5;
                        }

                    }

                    if (number > 0)
                    {
                        collisionTiles.Add(new CollisionTiles(number, new Rectangle(x * size, y * size, size, size)));
                    }

                    width = (x + 1) * size;

                    height = (y + 1) * size;
                }
        }
        public void BossGenerate(int[] BossTiles, int size)
        {
            // same idea as arenaGenerate but loads a diffrent tile instead of the grass/dirt tiles
            for (int x = 0; x < BossTiles[0]; x++)
                for (int y = 0; y < BossTiles[1]; y++)
                {
                    int number = 0;
                    if (x == 0 && y == 0)
                    {
                        number = 10;
                    }
                    if (y == 0)
                    {
                        if (x > 0 && x < BossTiles[0])
                        {
                            number = 10;
                        }
                        if (x == BossTiles[0] - 1)
                        {
                            number = 10;
                        }
                    }
                    if (y > 0 && y < BossTiles[1] - 1)
                    {
                        if (x == 0)
                        {
                            number = 10;
                        }
                        else if (x == BossTiles[0] - 1)
                        {
                            number = 10;
                        }
                        else
                        {
                            number = 0;
                        }
                    }
                    if (y == BossTiles[1] - 1)
                    {
                        if (x == 0)
                        {
                            number = 10;
                        }
                        else if (x == BossTiles[0] - 1)
                        {
                            number = 10;
                        }
                        else
                        {
                            number = 10;
                        }

                    }
                    if (number > 0)
                    {
                        collisionTiles.Add(new CollisionTiles(number, new Rectangle(x * size, y * size, size, size)));
                    }

                    width = (x + 1) * size;

                    height = (y + 1) * size;
                }

        }

        public void Draw(SpriteBatch spriteBatch, Rectangle Screen)
        {

            foreach (CollisionTiles Tile in collisionTiles)
            {

                Tile.Draw(spriteBatch);

            }
        }

    }
    static class RectangleHelper
    {
        /// <summary>
        ///         these set the conditions for whenever something touches the top of one of the tiles or something that contains a rectangele
        ///         
        ///         r1 is the rectangle that will call the function where as r2 is the target
        ///         
        ///         example of this would be  if the player rect touches the top of the tile then do this
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool TouchTopof(this Rectangle r1, Rectangle r2)
        {
            return (r1.Bottom >= r2.Top &&
                    r1.Bottom <= r2.Top + (r2.Height / 2) &&
                    r1.Right >= r2.Left + (r2.Width / 5) &&
                    r1.Left <= r2.Right - (r2.Width / 5));
        }
        public static bool TouchBottomof(this Rectangle r1, Rectangle r2)
        {
            return (r1.Top <= r2.Bottom &&
                    r1.Top >= r2.Bottom - (r2.Height / 2) &&
                    r1.Right >= r2.Left + (r2.Width / 5) &&
                    r1.Left <= r2.Right - (r2.Width / 5));
        }
        public static bool TouchLeftof(this Rectangle r1, Rectangle r2)
        {
            return (r1.Right <= r2.Right &&
                r1.Right >= r2.Left - 5 &&
                r1.Top <= r2.Bottom - (r2.Width / 4) &&
                r1.Bottom >= r2.Top + (r2.Width / 4));
        }
        public static bool TouchRightof(this Rectangle r1, Rectangle r2)
        {
            return (r1.Left >= r2.Left &&
                r1.Left <= r2.Right + 5 &&
                r1.Top <= r2.Bottom - (r2.Width / 4) &&
                r1.Bottom >= r2.Top + (r2.Width / 4));
        }
    }


}
