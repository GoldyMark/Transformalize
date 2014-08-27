using System.Collections.Generic;
using Transformalize.Libs.Rhino.Etl;
using Transformalize.Libs.Rhino.Etl.Operations;

namespace Transformalize.Main.Providers.Lucene {
    public class LuceneExtract : AbstractOperation {

        private readonly LuceneConnection _luceneConnection;
        private readonly Entity _entity;
        private readonly Fields _fields;

        public LuceneExtract(LuceneConnection luceneConnection, Entity entity) {
            _luceneConnection = luceneConnection;
            _entity = entity;
            _fields = _entity.Fields.WithInput();
        }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows) {
            using (var reader = LuceneReaderFactory.Create(_luceneConnection, _entity, true)) {
                var docCount = reader.NumDocs();

                for (var i = 0; i < docCount; i++) {

                    if (reader.IsDeleted(i))
                        continue;

                    var doc = reader.Document(i);

                    var row = new Row();
                    foreach (var field in _fields) {
                        row[field.Alias] = Common.ConversionMap[field.SimpleType](doc.Get(field.Name));
                    }

                    yield return row;
                }
            }
        }
    }
}