using System;

namespace BirdyNetwork.Classes.Graph
{
    public class ObjectWithId
    {
        static int _newId;

        private readonly int _id;

        public int Id { get { return _id; } }

        static ObjectWithId()
        {
            _newId = 0;
        }

        public ObjectWithId()
        {
            _newId++;
            if (_newId == int.MaxValue) throw new Exception("Id максимального размера");
            _id = _newId;
        }
    }
}
