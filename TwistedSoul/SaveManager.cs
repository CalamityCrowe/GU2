using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TwistedSoul
{
    [Serializable]
    public struct PlayerStats
    {
        public int Level;

        public int Health;
        public int Mana;
        public int MaxHealth;
        public int MaxMana;
        
    }

    sealed class SaveManager
    {
        private PlayerStats _data;
        public PlayerStats DATA 
        {
            get 
            {
                return _data;
            }

        }
        private string _filename;

        public SaveManager(string fileName)
        {
            _filename = fileName;
            _data = new PlayerStats();
            if (!File.Exists(fileName))
            {

            }
            else
            {
                load();
            }
        }
        ~SaveManager()
        { 
            save();
        }

        public void Add(LEVEL Current,Player CurrentPlay)
        {

            _data.Health = CurrentPlay.HEALTH;
            _data.Mana = CurrentPlay.MANA;
            _data.MaxHealth = CurrentPlay.MAXHEALTH;
            _data.MaxMana = CurrentPlay.MAXMANA;

            _data.Level = Convert.ToInt32(Current);
        }
        public void RESET() 
        {
            _data.Health = 400;
            _data.Mana = 250;
            _data.MaxHealth = 400;
            _data.MaxMana = 250;

            _data.Level = 0;

        }



        private void save()
        {
            FileStream stream;

            try
            {
                //open the file, creating if necessary
                stream = File.Open(_filename, FileMode.OpenOrCreate);

                // Convert the object to XML data and put it in the stream
                XmlSerializer serializer = new XmlSerializer(typeof(PlayerStats));
                serializer.Serialize(stream, _data);

                // Close the file
                stream.Close();
            }
            catch(Exception error)
            {
                Debug.WriteLine("Save has failed because of: " + error.Message);

            }
        }
        private void load()
        {
            FileStream stream;

            try
            {
                // Open the file - but read only mode!
                stream = File.Open(_filename, FileMode.OpenOrCreate, FileAccess.Read);
                // Read the data from the file
                XmlSerializer serializer = new XmlSerializer(typeof(PlayerStats));
                _data = (PlayerStats)serializer.Deserialize(stream);
            }
            catch (Exception error) // The code in "catch" is what happens if the "try" fails.
            {
                Debug.WriteLine("Load has failed because of: " + error.Message);
            }
        }
    }

}
