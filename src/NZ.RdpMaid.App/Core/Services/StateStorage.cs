using System;
using System.Text.Json;
using NZ.RdpMaid.App.SerializationModels.StateModel;

namespace NZ.RdpMaid.App.Core.Services
{
    internal class StateStorage
    {
        private const string _fileName = "state.json";

        private readonly FileStorage _fileStorage;

        public StateStorage(FileStorage fileStorage)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
        }

        public RuntimeState Load()
        {
            if (!_fileStorage.FileExists(_fileName))
            {
                return new RuntimeState();
            }

            var content = _fileStorage.ReadTextFile(_fileName);
            var state = JsonSerializer.Deserialize<RuntimeState>(content);

            return state ?? new RuntimeState();
        }

        public void Save(RuntimeState state)
        {
            var content = JsonSerializer.Serialize(state);
            _fileStorage.CreateTextFile(_fileName, content);
        }
    }
}