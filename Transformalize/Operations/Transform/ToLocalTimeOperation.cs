using System;
using System.Collections.Generic;
using Transformalize.Libs.NLog.Internal;
using Transformalize.Libs.Rhino.Etl;
using Transformalize.Libs.Rhino.Etl.Operations;

namespace Transformalize.Operations.Transform {

    public class ToLocalTimeOperation : AbstractOperation {

        private readonly string _inKey;
        private readonly string _outKey;
        private readonly TimeSpan _adjustment;

        public ToLocalTimeOperation(string inKey, string outKey, string fromTimeZone, string toTimeZone) {
            _inKey = inKey;
            _outKey = outKey;

            fromTimeZone = GuardTimeZone(fromTimeZone, "UTC");
            toTimeZone = GuardTimeZone(toTimeZone, TimeZoneInfo.Local.Id);

            var fromTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(fromTimeZone);
            var toTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(toTimeZone);

            _adjustment = toTimeZoneInfo.BaseUtcOffset - fromTimeZoneInfo.BaseUtcOffset;

            if (toTimeZoneInfo.IsDaylightSavingTime(DateTime.Now)) {
                _adjustment = _adjustment.Add(new TimeSpan(0, 1, 0, 0));
            }

        }

        private string GuardTimeZone(string timeZone, string defaultTimeZone) {
            var result = timeZone;
            if (timeZone == string.Empty) {
                result = defaultTimeZone;
                Debug("Defaulting From TimeZone to {0}.", defaultTimeZone);
            } else {
                if (!TimeZoneInfo.GetSystemTimeZones().Any(tz => tz.Id.Equals(timeZone))) {
                    Error("From Timezone Id {0} is invalid.", timeZone);
                    Environment.Exit(1);
                }
            }
            return result;
        }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows) {
            foreach (var row in rows) {
                var date = (DateTime)row[_inKey];
                row[_outKey] = date.Add(_adjustment);
                yield return row;
            }
        }
    }
}