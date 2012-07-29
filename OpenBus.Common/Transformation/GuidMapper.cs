using System;
using System.Collections.Generic;
using log4net;
using OpenBus.Common.Serialization;

namespace OpenBus.Common.Transformation
{
    /// <summary>
    /// Helper class for mapping between local ids and globally used guids.
    /// Mappings are persistent and stored locally in two xml files.
    /// </summary>
    public static class GuidMapper
    {
        // Mapping tables
        private static Dictionary<string, Guid> _guids = new Dictionary<string, Guid>();
        private static Dictionary<Guid, string> _localIds = new Dictionary<Guid, string>();

        // Lock for mutual exclusion
        private static readonly object MyLock = new object();

        private static readonly ILog Logger = LogManager.GetLogger(typeof(GuidMapper));

        static GuidMapper()
        {
            Initialize();
        }

        private static void Initialize()
        {
            // Have we checked the xml files?
            _guids = XmlSerializer.DeserializeFromFile<Dictionary<string, Guid>>("Guids.xml");
            _localIds = XmlSerializer.DeserializeFromFile<Dictionary<Guid, string>>("LocalIds.xml");

            // The very first time we will always get null from deserialization
            if (_guids == null)
                _guids = new Dictionary<string, Guid>();

            if (_localIds == null)
                _localIds = new Dictionary<Guid, string>();
        }

        /// <summary>
        /// Maps a local id to a guid. Also ensures that a mapping exists.
        /// </summary>
        /// <param name="localId"></param>
        public static Guid GetGuid(string localId)
        {
            if (String.IsNullOrEmpty(localId))
            {
                Logger.Error("Could not find guid because provided localId was null or empty.");
                return Guid.Empty;
            }

            EnsureMappingExists(localId);

            Guid guid = Guid.Empty;

            // Check if we have the guid
            _guids.TryGetValue(localId, out guid);

            // Should never happen !!
            if (guid == Guid.Empty)
                Logger.Error(String.Format("Could not get guid for localId '{0}'.", localId));

            // We have it (hopefully), so return it
            return guid;
        }

        /// <summary>
        /// Ensures that a mapping exists for a given local id
        /// </summary>
        /// <param name="localId"></param>
        public static void EnsureMappingExists(string localId)
        {
            if (String.IsNullOrEmpty(localId))
            {
                Logger.Error("Could not ensure mapping because provided localId was null or empty.");
                return;
            }

            lock (MyLock)
            {
                // Check if we have the guid
                if (!_guids.ContainsKey(localId))
                {
                    Guid guid = Guid.NewGuid();

                    _guids.Add(localId, guid);
                    _localIds.Add(guid, localId);

                    // Store the mappings
                    XmlSerializer.SerializeToFile(_guids, "Guids.xml");
                    XmlSerializer.SerializeToFile(_localIds, "LocalIds.xml");
                }
            }
        }

        /// <summary>
        /// Maps a guid to a local id.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string GetLocalId(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                Logger.Error("Could not get local id because an empty guid was provided.");
                return null;
            }

            string localId;

            if (_localIds.TryGetValue(guid, out localId))
                return localId;

            Logger.Warn(String.Format("Could not find localId for guid '{0}'.", guid));
            return null;
        }
    }
}
