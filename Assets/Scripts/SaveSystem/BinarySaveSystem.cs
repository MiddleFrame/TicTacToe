using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace SaveSystem
{
    public class BinarySaveSystem : ISaveSystem
    {
        private readonly string _filePath;
        private readonly string _saveName;

        public BinarySaveSystem(string data)
        {
            _saveName = data;
            _filePath = Application.persistentDataPath + $"/{data}.dat";
            Debug.Log(
                $"[BinarySaveSystem] Init '{_saveName}' path='{_filePath}' platform={Application.platform}");
        }

        public void Save(ISaveble data)
        {
            try
            {
                using FileStream file = File.Create(_filePath);
                new BinaryFormatter().Serialize(file, data);
                file.Flush();

                long fileSize = new FileInfo(_filePath).Length;
                Debug.Log(
                    $"[BinarySaveSystem] Save OK '{_saveName}' type={data?.GetType().Name} bytes={fileSize} path='{_filePath}'");
            }
            catch (System.Exception ex)
            {
                Debug.LogError(
                    $"[BinarySaveSystem] Save FAILED '{_saveName}' path='{_filePath}' error={ex}");
                throw;
            }
        }

        public ISaveble Load()
        {
            try
            {
                ISaveble saveData = null;
                using FileStream file = File.Open(_filePath, FileMode.OpenOrCreate);
                if (file.Length != 0)
                {
                    file.Position = 0;
                    object loadedData = new BinaryFormatter().Deserialize(file);
                    saveData = (ISaveble)loadedData;

                    Debug.Log(
                        $"[BinarySaveSystem] Load OK '{_saveName}' type={loadedData?.GetType().Name} bytes={file.Length} path='{_filePath}'");
                }
                else
                {
                    Debug.Log(
                        $"[BinarySaveSystem] Load EMPTY '{_saveName}' path='{_filePath}'");
                }

                return saveData;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(
                    $"[BinarySaveSystem] Load FAILED '{_saveName}' path='{_filePath}' error={ex}");
                throw;
            }
        }

        public static void DeleteAllSaves()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Application.persistentDataPath);

            foreach (FileInfo file in dirInfo.GetFiles())
            {

                if (file.Name.EndsWith("dat"))
                {
                    Debug.Log("DELETE FILE " + file.Name);
                    file.Delete();
                }

            }
        }
    }
}