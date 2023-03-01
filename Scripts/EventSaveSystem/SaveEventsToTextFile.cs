using System;
using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// HOW TO USE
    ///
    /// If you want to save an event that happens you write like this in the method that executes the
    /// event that you want to save.
    ///
    /// SaveEventsToTextFile.Current.AddToTextFileServerRpc(" Write what happens "); Time stamps will be added.
    ///
    /// There are a one more option of the same AddToTextFileServerRPC call, that takes in the position where it happen
    ///   SaveEventsToTextFile.AddToTextFileServerRpc(string whatOccured, Vector3 pos)
    ///
    /// It could be a good ide to clear the file before you want to start some testing.
    ///
    /// When the game starts you get a Debug message that shows where the text file is saved that
    /// you can copy in to the file explorer.
    /// C:/Users/"Your user Name"/AppData/LocalLow/DefaultCompany/Overthrown/SavedEventData.txt
    ///
    /// </summary>
    public class SaveEventsToTextFile : NetworkBehaviour
    {
        [SerializeField] private CurrentSession currentSession;


        public static SaveEventsToTextFile Current;

        private void Awake()
        {
            Current = this;
            currentSession.SesNumber++;
        }


        private void Start()
        {
            WriteSessionToTextFile();
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddToTextFileServerRpc(string whatOccured, Vector3 pos)
        {
            string path = Application.persistentDataPath + "/SavedEventData.txt";

            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine($"{whatOccured};{pos};Time {DateTime.Now}\n ");
            writer.Close();
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddToTextFileServerRpc(string whatOccured)
        {
            string path = Application.persistentDataPath + "/SavedEventData.txt";

            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine($"{whatOccured};Time {DateTime.Now}\n ");
            writer.Close();
        }

        private void WriteSessionToTextFile()
        {
            string path = Application.persistentDataPath + "/SavedEventData.txt";

            StreamWriter writer = new StreamWriter(path, true);
            writer.Write($"CurrentSession {currentSession.SesNumber}\n++++++++++++++++++++++\n");
            writer.Close();
            Debug.Log(Application.persistentDataPath + "/SavedEventData.txt");
        }
    }
}