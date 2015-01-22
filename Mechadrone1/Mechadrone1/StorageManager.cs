using System;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;
using System.Collections.Generic;
using Mechadrone1.Screens;
using System.Xml.Serialization;

namespace Mechadrone1
{
    // TODO: P2: Hope that monogame can change the windows file save location
    // The runtime equivalent of the 'game save'.
    static class StorageManager
    {
        public const string SAVE_CONTAINER_NAME = "CampaignData";

        /// <summary>
        /// A delegate for receiving StorageDevice objects.
        /// </summary>
        public delegate void StorageDeviceDelegate(StorageDevice storageDevice);

        /// <summary>
        /// A delegate for callbacks when a save file has been loaded
        /// </summary>
        public delegate void DossierLoadedDelegate(GameDossier dossier);

        /// <summary>
        /// A delegate for callbacks when the save game descriptions have been collected.
        /// </summary>
        public delegate void SaveGameDescriptionsRetrievedDelegate(List<SaveGameDescription> saveGameDescriptions);

        /// <summary>
        /// XML serializer for SaveGameDescription objects.
        /// </summary>
        private static XmlSerializer sSaveGameDescriptionSerializer;

        static StorageManager()
        {
            sSaveGameDescriptionSerializer = new XmlSerializer(typeof(SaveGameDescription));
        }

        /// <summary>
        /// Refresh the list of save-game descriptions.
        /// </summary>
        public static void GetSaveGameDescriptions(SaveGameDescriptionsRetrievedDelegate saveGameDescriptionsRetrievedDelegate)
        {
            // retrieve the storage device, asynchronously
            GetStorageDevice(delegate(StorageDevice storageDevice)
                {
                    List<SaveGameDescription> saveGameDescriptions = RefreshSaveGameDescriptionsResult(storageDevice);
                    saveGameDescriptionsRetrievedDelegate(saveGameDescriptions);
                });
        }

        /// <summary>
        /// Asynchronous storage-device callback for 
        /// refreshing the save-game descriptions.
        /// </summary>
        private static List<SaveGameDescription> RefreshSaveGameDescriptionsResult(StorageDevice storageDevice)
        {
            if (storageDevice == null)
                throw new ArgumentNullException("storageDevice");

            if (!storageDevice.IsConnected)
                throw new InvalidOperationException("Cannot connect to storage device.");

            List<SaveGameDescription> saveGameDescriptions = new List<SaveGameDescription>();

            // open the container
            using (StorageContainer storageContainer = OpenContainer(storageDevice))
            {
                // get the description list 
                string[] filenames = storageContainer.GetFileNames("SaveGameDescription*.xml");
                // add each entry to the list
                foreach (string filename in filenames)
                {
                    SaveGameDescription saveGameDescription;

                    // check the size of the list
                    if (saveGameDescriptions.Count >= SaveLoadScreen.MaximumSaveGameDescriptions)
                        break;

                    // open the file stream
                    using (Stream fileStream = storageContainer.OpenFile(filename, FileMode.Open))
                    {
                        // deserialize the object
                        saveGameDescription = sSaveGameDescriptionSerializer.Deserialize(fileStream) as SaveGameDescription;
                        // if it's valid, add it to the list
                        if (saveGameDescription != null)
                            saveGameDescriptions.Add(saveGameDescription);
                    }
                }
            }

            return saveGameDescriptions;
        }

        public static void LoadFromSaveFile(SaveGameDescription saveGameDescription, DossierLoadedDelegate dossierLoadedCallback)
        {
            if (saveGameDescription == null)
                throw new ArgumentNullException("saveGameDescription");
            if (dossierLoadedCallback == null)
                throw new ArgumentNullException("dossierLoadedCallback");

            // get the storage device and load the session
            GetStorageDevice(delegate(StorageDevice storageDevice)
                {
                    GameDossier loadedDossier = LoadDossierResult(storageDevice, saveGameDescription);
                    dossierLoadedCallback(loadedDossier);
                });
        }

        /// <summary>
        /// Asynchronously retrieve a storage device.
        /// </summary>
        /// <param name="retrievalDelegate">
        /// The delegate called when the device is available.
        /// </param>
        /// <remarks>
        /// If there is a suitable cached storage device, 
        /// the delegate may be called directly by this function.
        /// </remarks>
        public static void GetStorageDevice(StorageDeviceDelegate retrievalDelegate)
        {
            if (retrievalDelegate == null)
                throw new ArgumentNullException("retrievalDelegate");

            // If the storage device is ready, we can just retrieve the file:
            if ((SharedResources.StorageDevice != null) && SharedResources.StorageDevice.IsConnected)
            {
                retrievalDelegate(SharedResources.StorageDevice);
                return;
            }

            // The storage device must be refreshed
            if (!Guide.IsVisible)
            {
                // Reset the device
                SharedResources.StorageDevice = null;
                StorageDevice.BeginShowSelector(GetStorageDeviceResult, retrievalDelegate);
            }
        }

        /// <summary>
        /// Asynchronous callback to the guide's BeginShowStorageDeviceSelector call.
        /// </summary>
        /// <param name="result">The IAsyncResult object with the device.</param>
        private static void GetStorageDeviceResult(IAsyncResult result)
        {
            // check the parameter
            if ((result == null) || !result.IsCompleted)
                return;

            // retrieve and store the storage device
            SharedResources.StorageDevice = StorageDevice.EndShowSelector(result);

            // check the new storage device 
            if ((SharedResources.StorageDevice != null) && SharedResources.StorageDevice.IsConnected)
            {
                // it passes; call the stored delegate
                StorageDeviceDelegate func = result.AsyncState as StorageDeviceDelegate;
                if (func != null)
                {
                    func(SharedResources.StorageDevice);
                }
            }
        }

        /// <summary>
        /// Receives the storage device and starts a new session, 
        /// using the data in the given save game.
        /// </summary>
        /// <remarks>The new session is created in LoadSessionResult.</remarks>
        /// <param name="storageDevice">The chosen storage device.</param>
        /// <param name="saveGameDescription">The description of the save game.</param>
        private static GameDossier LoadDossierResult(StorageDevice storageDevice, SaveGameDescription saveGameDescription)
        {
            if (saveGameDescription == null)
                throw new ArgumentNullException("saveGameDescription");

            if (storageDevice == null)
                throw new ArgumentNullException("storageDevice");

            if (!storageDevice.IsConnected)
                throw new InvalidOperationException("Cannot connect to storage device.");

            GameDossier loadedDossier = new GameDossier();
            // open the container
            using (StorageContainer storageContainer = OpenContainer(storageDevice))
            {
                using (Stream stream = storageContainer.OpenFile(saveGameDescription.FileName, FileMode.Open))
                {
                    using (XmlReader xmlReader = XmlReader.Create(stream))
                    {
                        // <Mechadrone1SaveData>
                        xmlReader.ReadStartElement("Mechadrone1SaveData");

                        /*
                        // read the map information
                        xmlReader.ReadStartElement("mapData");
                        string mapAssetName =
                            xmlReader.ReadElementString("mapContentName");
                        PlayerPosition playerPosition = new XmlSerializer(
                            typeof(PlayerPosition)).Deserialize(xmlReader)
                            as PlayerPosition;
                        singleton.removedMapChests = new XmlSerializer(
                            typeof(List<WorldEntry<Chest>>)).Deserialize(xmlReader)
                            as List<WorldEntry<Chest>>;
                        singleton.removedMapFixedCombats = new XmlSerializer(
                            typeof(List<WorldEntry<FixedCombat>>)).Deserialize(xmlReader)
                            as List<WorldEntry<FixedCombat>>;
                        singleton.removedMapPlayerNpcs = new XmlSerializer(
                            typeof(List<WorldEntry<Player>>)).Deserialize(xmlReader)
                            as List<WorldEntry<Player>>;
                        singleton.modifiedMapChests = new XmlSerializer(
                            typeof(List<ModifiedChestEntry>)).Deserialize(xmlReader)
                            as List<ModifiedChestEntry>;
                        ChangeMap(mapAssetName, null);
                        TileEngine.PartyLeaderPosition = playerPosition;
                        xmlReader.ReadEndElement();

                        // read the quest information
                        ContentManager content = Session.ScreenManager.Game.Content;
                        xmlReader.ReadStartElement("questData");
                        singleton.questLine = content.Load<QuestLine>(
                            xmlReader.ReadElementString("questLineContentName")).Clone()
                            as QuestLine;
                        singleton.currentQuestIndex = Convert.ToInt32(
                            xmlReader.ReadElementString("currentQuestIndex"));
                        for (int i = 0; i < singleton.currentQuestIndex; i++)
                        {
                            singleton.questLine.Quests[i].Stage =
                                Quest.QuestStage.Completed;
                        }
                        singleton.removedQuestChests = new XmlSerializer(
                            typeof(List<WorldEntry<Chest>>)).Deserialize(xmlReader)
                            as List<WorldEntry<Chest>>;
                        singleton.removedQuestFixedCombats = new XmlSerializer(
                            typeof(List<WorldEntry<FixedCombat>>)).Deserialize(xmlReader)
                            as List<WorldEntry<FixedCombat>>;
                        singleton.modifiedQuestChests = new XmlSerializer(
                            typeof(List<ModifiedChestEntry>)).Deserialize(xmlReader)
                            as List<ModifiedChestEntry>;
                        Quest.QuestStage questStage = (Quest.QuestStage)Enum.Parse(
                            typeof(Quest.QuestStage),
                            xmlReader.ReadElementString("currentQuestStage"), true);
                        if ((singleton.questLine != null) && !IsQuestLineComplete)
                        {
                            singleton.quest =
                                singleton.questLine.Quests[CurrentQuestIndex];
                            singleton.ModifyQuest(singleton.quest);
                            singleton.quest.Stage = questStage;
                        }
                        xmlReader.ReadEndElement();

                        // read the party data
                        singleton.party = new Party(new XmlSerializer(
                            typeof(PartySaveData)).Deserialize(xmlReader)
                            as PartySaveData, content);
                        */

                        // </Mechadrone1SaveData>
                        xmlReader.ReadEndElement();
                    }
                }
            }

            return loadedDossier;
        }

        /// <summary>
        /// Synchronously opens storage container
        /// </summary>
        private static StorageContainer OpenContainer(StorageDevice storageDevice)
        {
            IAsyncResult result = storageDevice.BeginOpenContainer(SAVE_CONTAINER_NAME, null, null);

            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();

            StorageContainer container = storageDevice.EndOpenContainer(result);

            // Close the wait handle.
            result.AsyncWaitHandle.Close();

            return container;
        }

        /// <summary>
        /// Save the current state of the session.
        /// </summary>
        /// <param name="overwriteDescription">
        /// The description of the save game to over-write, if any.
        /// </param>
        public static void SaveDossier(SaveGameDescription overwriteDescription)
        {
            // retrieve the storage device, asynchronously
            GetStorageDevice(delegate(StorageDevice storageDevice)
                {
                    SaveDossierResult(storageDevice, overwriteDescription);
                });
        }

        /// <summary>
        /// Save the current state of the session, with the given storage device.
        /// </summary>
        /// <param name="storageDevice">The chosen storage device.</param>
        /// <param name="overwriteDescription">
        /// The description of the save game to over-write, if any.
        /// </param>
        private static void SaveDossierResult(StorageDevice storageDevice, SaveGameDescription overwriteDescription)
        {
            if (storageDevice == null)
                throw new ArgumentNullException("storageDevice");

            if (!storageDevice.IsConnected)
                throw new InvalidOperationException("Cannot connect to storage device.");

            // open the container
            using (StorageContainer storageContainer = OpenContainer(storageDevice))
            {
                string filename;
                string descriptionFilename;
                // get the filenames
                if (overwriteDescription == null)
                {
                    int saveGameIndex = 0;
                    string testFilename;
                    do
                    {
                        saveGameIndex++;
                        testFilename = "SaveGame" + saveGameIndex.ToString() + ".xml";
                    }
                    while (storageContainer.FileExists(testFilename));
                    filename = testFilename;
                    descriptionFilename = "SaveGameDescription" + saveGameIndex.ToString() + ".xml";
                }
                else
                {
                    filename = overwriteDescription.FileName;
                    descriptionFilename = "SaveGameDescription" + Path.GetFileNameWithoutExtension(
                        overwriteDescription.FileName).Substring(8) + ".xml";
                }
                using (Stream stream = storageContainer.OpenFile(filename, FileMode.Create))
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(stream))
                    {
                        // <Mechadrone1SaveData>
                        xmlWriter.WriteStartElement("Mechadrone1SaveData");
                        /*
                        // write the map information
                        xmlWriter.WriteStartElement("mapData");
                        xmlWriter.WriteElementString("mapContentName",
                            TileEngine.Map.AssetName);
                        new XmlSerializer(typeof(PlayerPosition)).Serialize(
                            xmlWriter, TileEngine.PartyLeaderPosition);
                        new XmlSerializer(typeof(List<WorldEntry<Chest>>)).Serialize(
                            xmlWriter, singleton.removedMapChests);
                        new XmlSerializer(
                            typeof(List<WorldEntry<FixedCombat>>)).Serialize(
                            xmlWriter, singleton.removedMapFixedCombats);
                        new XmlSerializer(typeof(List<WorldEntry<Player>>)).Serialize(
                            xmlWriter, singleton.removedMapPlayerNpcs);
                        new XmlSerializer(typeof(List<ModifiedChestEntry>)).Serialize(
                            xmlWriter, singleton.modifiedMapChests);
                        xmlWriter.WriteEndElement();

                        // write the quest information
                        xmlWriter.WriteStartElement("questData");
                        xmlWriter.WriteElementString("questLineContentName",
                            singleton.questLine.AssetName);
                        xmlWriter.WriteElementString("currentQuestIndex",
                            singleton.currentQuestIndex.ToString());
                        new XmlSerializer(typeof(List<WorldEntry<Chest>>)).Serialize(
                            xmlWriter, singleton.removedQuestChests);
                        new XmlSerializer(
                            typeof(List<WorldEntry<FixedCombat>>)).Serialize(
                            xmlWriter, singleton.removedQuestFixedCombats);
                        new XmlSerializer(typeof(List<ModifiedChestEntry>)).Serialize(
                            xmlWriter, singleton.modifiedQuestChests);
                        xmlWriter.WriteElementString("currentQuestStage",
                            IsQuestLineComplete ?
                            Quest.QuestStage.NotStarted.ToString() :
                            singleton.quest.Stage.ToString());
                        xmlWriter.WriteEndElement();

                        // write the party data
                        new XmlSerializer(typeof(PartySaveData)).Serialize(xmlWriter,
                            new PartySaveData(singleton.party));
                        */
                        // </Mechadrone1SaveData>
                        xmlWriter.WriteEndElement();
                    }
                }

                // create the save game description
                SaveGameDescription description = new SaveGameDescription();
                description.FileName = Path.GetFileName(filename);
                description.ChapterName = "01 - Harrowmark";
                description.Description = DateTime.Now.ToString();
                using (Stream stream = storageContainer.OpenFile(descriptionFilename, FileMode.Create))
                {
                    new XmlSerializer(typeof(SaveGameDescription)).Serialize(stream, description);
                }
            }
        }

        /// <summary>
        /// Delete the save game specified by the description.
        /// </summary>
        /// <param name="saveGameDescription">The description of the save game.</param>
        public static void DeleteSaveGame(SaveGameDescription saveGameDescription)
        {
            if (saveGameDescription == null)
                throw new ArgumentNullException("saveGameDescription");

            // get the storage device and load the session
            GetStorageDevice(
                delegate(StorageDevice storageDevice)
                {
                    DeleteSaveGameResult(storageDevice, saveGameDescription);
                });
        }

        /// <summary>
        /// Delete the save game specified by the description.
        /// </summary>
        /// <param name="storageDevice">The chosen storage device.</param>
        /// <param name="saveGameDescription">The description of the save game.</param>
        public static void DeleteSaveGameResult(StorageDevice storageDevice, SaveGameDescription saveGameDescription)
        {
            if (saveGameDescription == null)
                throw new ArgumentNullException("saveGameDescription");

            if (storageDevice == null)
                throw new ArgumentNullException("storageDevice");

            if (!storageDevice.IsConnected)
                throw new InvalidOperationException("Cannot connect to storage device.");

            // open the container
            using (StorageContainer storageContainer = OpenContainer(storageDevice))
            {
                storageContainer.DeleteFile(saveGameDescription.FileName);
                storageContainer.DeleteFile("SaveGameDescription" +
                    Path.GetFileNameWithoutExtension(saveGameDescription.FileName).Substring(8) + ".xml");
            }
        }
    }
}
