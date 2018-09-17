﻿#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests {

    [TestClass]
    public class TestFormat {

        [TestMethod]
        public void FormatTransformer() {

            const string xml = @"
    <add name='TestProcess'>
      <entities>
        <add name='TestData'>
          <rows>
            <add Field1='1' Field2='2' Field3='3' />
          </rows>
          <fields>
            <add name='Field1' />
            <add name='Field2' />
            <add name='Field3' type='double' />
          </fields>
          <calculated-fields>
            <add name='Format' t='copy(Field1,Field2,Field3).format({0}-{1}+{2} ).trim()' />
            <add name='BetterFormat' t='format({Field1}-{Field2}+{Field3} ).trim()' />
            <add name='FormatRepeats' t='copy(Field1,Field2,Field3).format({0}-{1}+{2}_{1})' />
            <add name='BetterFormatRepeats' t='format({Field1}-{Field2}+{Field3}_{Field2})' />
            <add name='WithFormat' t='format({Field1:#.0} and {Field3:000.0000})' />
          </calculated-fields>
        </add>
      </entities>
    </add>";

            var composer = new CompositionRoot();
            var controller = composer.Compose(xml);
            var output = controller.Read().ToArray();

            Assert.AreEqual("1-2+3", output[0][composer.Process.Entities.First().CalculatedFields.First(f=>f.Name == "Format")]);
            Assert.AreEqual("1-2+3", output[0][composer.Process.Entities.First().CalculatedFields.First(f=>f.Name == "BetterFormat")]);
            Assert.AreEqual("1-2+3_2", output[0][composer.Process.Entities.First().CalculatedFields.First(f => f.Name == "FormatRepeats")]);
            Assert.AreEqual("1-2+3_2", output[0][composer.Process.Entities.First().CalculatedFields.First(f => f.Name == "BetterFormatRepeats")]);
            Assert.AreEqual("1 and 003.0000", output[0][composer.Process.Entities.First().CalculatedFields.First(f => f.Name == "WithFormat")]);

        }
    }
}
