using Transformalize.Configuration;
using Transformalize.Libs.Rhino.Etl.Operations;

namespace Transformalize.Main.Providers.Folder {

    public class FolderConnection : AbstractConnection {

        public override string UserProperty { get { return string.Empty; } }
        public override string PasswordProperty { get { return string.Empty; } }
        public override string PortProperty { get { return string.Empty; } }
        public override string DatabaseProperty { get { return string.Empty; } }
        public override string ServerProperty { get { return string.Empty; } }
        public override string TrustedProperty { get { return string.Empty; } }
        public override string PersistSecurityInfoProperty { get { return string.Empty; } }
        public override int NextBatchId(string processName) {
            return 1;
        }

        public override void WriteEndVersion(AbstractConnection input, Entity entity) {
            throw new System.NotImplementedException();
        }

        public override IOperation EntityOutputKeysExtract(Entity entity) {
            throw new System.NotImplementedException();
        }

        public override IOperation EntityOutputKeysExtractAll(Entity entity) {
            throw new System.NotImplementedException();
        }

        public override IOperation EntityBulkLoad(Entity entity) {
            throw new System.NotImplementedException();
        }

        public override IOperation EntityBatchUpdate(Entity entity) {
            throw new System.NotImplementedException();
        }

        public FolderConnection(Process process, ConnectionConfigurationElement element, AbstractConnectionDependencies dependencies)
            : base(element, dependencies) {

            TypeAndAssemblyName = process.Providers[element.Provider.ToLower()];
            Type = ProviderType.Folder;
        }

    }
}