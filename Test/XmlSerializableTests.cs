using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serialization.Xml.Internal;

namespace Serialization.Xml.Test {
    [TestClass]
    public class XmlSerializableTests {

        private T RoundTrip<T>(T src) {
            using (var buffer = new MemoryStream()) {
                using (var writer = XmlTextWriter.Create(buffer, new XmlWriterSettings { Indent = true, IndentChars = "    " })) {
                    XmlSerializable.Serialize(writer, src);
                }

                buffer.Position = 0;
                using (var reader = XmlTextReader.Create(buffer)) {
                    return XmlSerializable.Deserialize<T>(reader);
                }
            }
        }

        private T2 BackwardCompatRoundTrip<T1, T2>(T1 src, bool useLegacySerializer = true)
            where T2 : XmlSerializable {

            using (var buffer = new MemoryStream()) {
                using (var writer = XmlTextWriter.Create(buffer, new XmlWriterSettings { Indent = true, IndentChars = "    " })) {
                    if (useLegacySerializer) {
                        var serializer = XmlSerializer.FromTypes(new[] { typeof(T1) })[0];
                        serializer.Serialize(writer, src);
                    } else {
                        XmlSerializable.Serialize<T1>(writer, src);
                    }
                }

                buffer.Position = 0;
                using (var reader = XmlTextReader.Create(buffer)) {
                    return XmlSerializable.Deserialize<T2>(reader);
                }
            }
        }

        private void Validate<T>(T src, T dest) {
            foreach (var member in typeof(T).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                object srcval, destval;
                if (member is PropertyInfo) {
                    var property = (PropertyInfo)member;
                    // Ignore index properties
                    var indexParameters = property.GetIndexParameters();
                    if (indexParameters == null || !indexParameters.Any()) {
                        srcval = property.GetValue(src, null);
                        destval = property.GetValue(dest, null);
                    }
                } else if (member is FieldInfo) {
                    var field = (FieldInfo)member;
                }
            }
        }

        private T Deserialize<T>(string xml) where T : XmlSerializable {
            using (var buffer = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
                return XmlSerializable.Deserialize<T>(buffer);
            }
        }

        private String Serialize<T>(T obj) where T : XmlSerializable {
            using (var buffer = new MemoryStream()) {
                XmlSerializable.Serialize(buffer, obj);
                var data = buffer.GetBuffer();
                return Encoding.UTF8.GetString(data, 0, (int)buffer.Length);
            }
        }

        [TestMethod]
        public void InstanceTest() {
            var obj = new InstanceSerializable { Foo = "Asdf" };
            var res = RoundTrip(obj);
            Validate(obj, res);
        }

        private class InstanceSerializable {
            public String Foo { get; set; }
        }

        [TestMethod]
        public void InheritanceTest() {
            var obj = new InheritanceSerializable { Foo = "Asdf" };
            var res = RoundTrip(obj);
            Validate(obj, res);
        }

        private class InheritanceSerializable : XmlSerializable<InheritanceSerializable> {
            public String Foo { get; set; }
        }

        [TestMethod]
        public void SimpleElementTypes() {
            var obj = InitializeSimpleTypes(new SimpleElementTypesSerializable());
            var res = RoundTrip(obj);
            Validate(obj, res);
        }

        #region SimpleElementTypes

        private T InitializeSimpleTypes<T>(T obj) where T : ISimpleTypes {
            obj.Bool = true;
            obj.Char = 'ɸ';
            obj.Byte = 128;
            obj.SByte = -64;
            obj.Short = -32000;
            obj.UShort = 65000;
            obj.Int = -2100000000;
            obj.UInt = 4200000000;
            obj.Long = -1000000000000000L;
            obj.ULong = 1000000000000000UL;
            obj.Single = 3.14159274f;
            obj.Double = 3.1415926535897931d;
            obj.Decimal = 3.1415926535897932384626433832795m;
            obj.DateTime = new DateTime(1938, 1, 10, 10, 13, 42, DateTimeKind.Utc);
            obj.TimeSpan = TimeSpan.FromHours(881.5);
            obj.Guid = Guid.NewGuid();
            obj.Enum = SerializableEnum.Bas;
            obj.Version = new Version(1, 1, 2, 3);
            return obj;
        }

        private class SimpleElementTypesSerializable : XmlSerializable<SimpleElementTypesSerializable>, ISimpleTypes {
            public Boolean Bool { get; set; }
            public Char Char { get; set; }
            public Byte Byte { get; set; }
            public SByte SByte { get; set; }
            public Int16 Short { get; set; }
            public UInt16 UShort { get; set; }
            public Int32 Int { get; set; }
            public UInt32 UInt { get; set; }
            public Int64 Long { get; set; }
            public UInt64 ULong { get; set; }
            public Single Single { get; set; }
            public Double Double { get; set; }
            public Decimal Decimal { get; set; }
            public DateTime DateTime { get; set; }
            public TimeSpan TimeSpan { get; set; }
            public Guid Guid { get; set; }
            public SerializableEnum Enum { get; set; }
            public Version Version { get; set; }
        }

        private interface ISimpleTypes {
            Boolean Bool { get; set; }
            Char Char { get; set; }
            Byte Byte { get; set; }
            SByte SByte { get; set; }
            Int16 Short { get; set; }
            UInt16 UShort { get; set; }
            Int32 Int { get; set; }
            UInt32 UInt { get; set; }
            Int64 Long { get; set; }
            UInt64 ULong { get; set; }
            Single Single { get; set; }
            Double Double { get; set; }
            Decimal Decimal { get; set; }
            DateTime DateTime { get; set; }
            TimeSpan TimeSpan { get; set; }
            Guid Guid { get; set; }
            SerializableEnum Enum { get; set; }
            Version Version { get; set; }
        }

        private enum SerializableEnum {
            Foo,
            Bar,
            Bas
        }

        #endregion

        [TestMethod]
        public void SimpleAttributeTypes() {
            var obj = InitializeSimpleTypes(new SimpleAttributeTypesSerializable());
            var res = RoundTrip(obj);
            Validate(obj, res);
        }

        #region SimpleAttributeTypes

        private class SimpleAttributeTypesSerializable : XmlSerializable<SimpleAttributeTypesSerializable>, ISimpleTypes {
            [XmlAttribute]
            public Boolean Bool { get; set; }
            [XmlAttribute]
            public Char Char { get; set; }
            [XmlAttribute]
            public Byte Byte { get; set; }
            [XmlAttribute]
            public SByte SByte { get; set; }
            [XmlAttribute]
            public Int16 Short { get; set; }
            [XmlAttribute]
            public UInt16 UShort { get; set; }
            [XmlAttribute]
            public Int32 Int { get; set; }
            [XmlAttribute]
            public UInt32 UInt { get; set; }
            [XmlAttribute]
            public Int64 Long { get; set; }
            [XmlAttribute]
            public UInt64 ULong { get; set; }
            [XmlAttribute]
            public Single Single { get; set; }
            [XmlAttribute]
            public Double Double { get; set; }
            [XmlAttribute]
            public Decimal Decimal { get; set; }
            [XmlAttribute]
            public DateTime DateTime { get; set; }
            [XmlAttribute]
            public TimeSpan TimeSpan { get; set; }
            [XmlAttribute]
            public Guid Guid { get; set; }
            [XmlAttribute]
            public SerializableEnum Enum { get; set; }
            [XmlAttribute]
            public Version Version { get; set; }
        }

        #endregion

        [TestMethod]
        public void Namespaces() {
            System.Diagnostics.Debug.WriteLine("Debug");
            Console.WriteLine("Console");
            var obj = Deserialize<CustomNamespaceSerializable>(ValidNamespacesXml);
            Assert.AreEqual("Asdf", obj.Foo);
            Assert.AreEqual("Qwert", obj.Bar);
            Assert.IsNotNull(obj.Bas);
            Assert.AreEqual("Zxcv", obj.Bas.Value);
            Assert.AreEqual("Tyuio", obj.Rab);
        }

        [TestMethod, ExpectedException(typeof(XmlException))]
        public void InvalidNamespaces() {
            var obj = Deserialize<CustomNamespaceSerializable>(InvalidNamespacesXml);
            Assert.IsNull(obj.Foo);
            Assert.AreEqual("Qwert", obj.Bar);
            Assert.IsNull(obj.Bas);
            Assert.IsNull(obj.Rab);
        }

        [TestMethod, ExpectedException(typeof(XmlException))]
        public void NoNamespaces() {
            var obj = Deserialize<CustomNamespaceSerializable>(NoNamespacesXml);
            Assert.IsNull(obj.Foo);
            Assert.AreEqual("Qwert", obj.Bar);
            Assert.IsNull(obj.Bas);
            Assert.IsNull(obj.Rab);
        }

        [TestMethod]
        public void XmlSerializerNamespaces() {
            var obj = XmlSerializerDeserialize<CustomNamespaceSerializable_>(ValidNamespacesXml);
            ValidateNamespacesValues(obj);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void XmlSerializerInvalidNamespaces() {
            var obj = XmlSerializerDeserialize<CustomNamespaceSerializable_>(InvalidNamespacesXml);
            Assert.IsNull(obj.Foo);
            Assert.AreEqual("Qwert", obj.Bar);
            Assert.IsNull(obj.Bas);
            Assert.IsNull(obj.Rab);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void XmlSerializerNoNamespaces() {
            var obj = XmlSerializerDeserialize<CustomNamespaceSerializable_>(NoNamespacesXml);
        }

        private static T XmlSerializerDeserialize<T>(string xml) {
            using (var buffer = new MemoryStream(Encoding.UTF8.GetBytes(xml))) {
                return (T)XmlSerializer.FromTypes(new [] { typeof(CustomNamespaceSerializable_) })[0].Deserialize(buffer);
            }
        }

        private String XmlSerializerSerialize<T>(T obj) {
            using (var buffer = new MemoryStream()) {
                XmlSerializer.FromTypes(new [] { typeof(CustomNamespaceSerializable_) })[0].Serialize(buffer, obj);
                var data = buffer.GetBuffer();
                return Encoding.UTF8.GetString(data, 0, (int)buffer.Length);
            }
        }

        [TestMethod]
        public void NamespacesCompatibility() {
            var us =
                new CustomNamespaceSerializable {
                    Foo = "Asdf",
                    Bar = "Qwert",
                    Bas = new CustomNamespaceChildSerializable { Attr = "11234", Value = "Zxcv" },
                    Rab = "Tyuio"
                };

            var them = XmlSerializerDeserialize<CustomNamespaceSerializable_>(Serialize(us));
            ValidateNamespacesValues(them);

            them =
                new CustomNamespaceSerializable_ {
                    Foo = "Asdf",
                    Bar = "Qwert",
                    Bas = new CustomNamespaceChildSerializable_ { Attr = "11234", Value = "Zxcv" },
                    Rab = "Tyuio"
                };

            us = Deserialize<CustomNamespaceSerializable>(XmlSerializerSerialize(them));
            ValidateNamespacesValues(us);
        }

        #region Namespaces

        private const String ValidNamespacesXml = @"
            <root f:Foo=""Asdf"" Bar=""Qwert"" xmlns=""http://test/root#"" xmlns:f=""http://test/foo#"">
                <b:Bas xmlns:b=""http://test/bas#"">
                    <b:Value>Zxcv</b:Value>
                </b:Bas>
                <Rab>Tyuio</Rab>
            </root>
        ";

        private const String InvalidNamespacesXml = @"
            <root f:Foo=""Asdf"" Bar=""Qwert"" xmlns=""http://test/toor#"" xmlns:f=""http://test/oof#"">
                <b:Bas xmlns:b=""http://test/sab#"">
                    <b:Value>Zxcv</b:Value>
                </b:Bas>
                <Rab>Tyuio</Rab>
            </root>
        ";

        private const String NoNamespacesXml = @"
            <root Foo=""Asdf"" Bar=""Qwert"">
                <Bas>
                    <Value>Zxcv</Value>
                </Bas>
                <Rab>Tyuio</Rab>
            </root>
        ";

        [XmlRoot("root", Namespace = "http://test/root#")]
        private sealed class CustomNamespaceSerializable : XmlSerializable<CustomNamespaceSerializable> {
            [XmlAttribute(Namespace = "http://test/foo#")]
            public String Foo { get; set; }

            // Defualt namespace
            [XmlAttribute]
            public String Bar { get; set; }

            [XmlElement(Namespace = "http://test/bas#")]
            public CustomNamespaceChildSerializable Bas { get; set; }

            public String Rab { get; set; }
        }

        private sealed class CustomNamespaceChildSerializable : XmlSerializable<CustomNamespaceChildSerializable> {
            [XmlAttribute]
            public String Attr { get; set; }
            public String Value { get; set; }
        }

        private static void ValidateNamespacesValues(CustomNamespaceSerializable obj) {
            Assert.AreEqual("Asdf", obj.Foo);
            Assert.AreEqual("Qwert", obj.Bar);
            Assert.IsNotNull(obj.Bas);
            Assert.AreEqual("Zxcv", obj.Bas.Value);
            Assert.AreEqual("Tyuio", obj.Rab);
        }

        [XmlRoot("root", Namespace = "http://test/root#")]
        public sealed class CustomNamespaceSerializable_ {
            [XmlAttribute(Namespace = "http://test/foo#")]
            public String Foo { get; set; }

            // Defualt namespace
            [XmlAttribute]
            public String Bar { get; set; }

            [XmlElement(Namespace = "http://test/bas#")]
            public CustomNamespaceChildSerializable_ Bas { get; set; }

            public String Rab { get; set; }
        }

        public sealed class CustomNamespaceChildSerializable_ {
            [XmlAttribute]
            public String Attr { get; set; }
            public String Value { get; set; }
        }

        private static void ValidateNamespacesValues(CustomNamespaceSerializable_ obj) {
            Assert.AreEqual("Asdf", obj.Foo);
            Assert.AreEqual("Qwert", obj.Bar);
            Assert.IsNotNull(obj.Bas);
            Assert.AreEqual("Zxcv", obj.Bas.Value);
            Assert.AreEqual("Tyuio", obj.Rab);
        }

        #endregion

        [TestMethod]
        public void Ordered() {
            var obj = Deserialize<OrderedSerializable>(OrderedXml);
            ValidateOrdered(obj);
        }

        private static void ValidateOrdered(OrderedSerializable obj) {
            Assert.IsNotNull(obj);
            Assert.AreEqual("2", obj.A);
            Assert.AreEqual("4", obj.B);
            Assert.AreEqual("6", obj.C);
            Assert.AreEqual("8", obj.D);
            Assert.AreEqual("Asdf", obj.Foo.Trim());
        }

        [TestMethod]
        public void OrderedRoundTrip() {
            var obj = RoundTrip(new OrderedSerializable { A = "2", B = "4", C = "6", D = "8", Foo = "Asdf" });
            ValidateOrdered(obj);
        }

        [TestMethod]
        public void AltOrdered() {
            var obj = Deserialize<OrderedSerializable>(AltOrderedXml);
            ValidateOrdered(obj);
        }

        [TestMethod]
        public void OrderedWithIgnoredText() {
            var obj = Deserialize<OrderedSerializable>(OrderedWithIgnoredTextXml);
            ValidateOrdered(obj);
        }

        [TestMethod]
        public void OutOfOrderText() {
            var obj = Deserialize<OrderedSerializable>(OutOfOrderXml);
            Assert.IsNotNull(obj);
            Assert.AreEqual("2", obj.A);
            Assert.AreEqual(null, obj.B);
            Assert.AreEqual("6", obj.C);
            Assert.AreEqual("8", obj.D);
            Assert.AreEqual("Asdf", obj.Foo.Trim());
        }

        #region Ordered

        private const string OrderedXml = @"
            <ordered>
                <A>2</A>
                <B>4</B>
                <C>6</C>
                <D>8</D>
                Asdf
            </ordered>";

        private const string AltOrderedXml = @"
            <ordered>
                <A>2</A>
                <C>6</C>
                <B>4</B>
                <D>8</D>
                Asdf
            </ordered>";

        private const string OutOfOrderXml = @"
            <ordered>
                <A>2</A>
                <C>6</C>
                <D>8</D>
                <B>4</B><!-- this wil be ignored -->
                Asdf
            </ordered>";

        private const string OrderedWithIgnoredTextXml = @"
            <ordered>
                Ignored1
                <A>2</A>
                Ignored2
                <B>4</B>
                Ignored3
                <C>6</C>
                Ignored4
                <D>8</D>
                Asdf
            </ordered>";

        #endregion

        [XmlRoot(ElementName = "ordered")]
        public class OrderedSerializable : XmlSerializable<OrderedSerializable> {
            [XmlElement(Order = 0)]
            public String A { get; set; }

            [XmlElement(Order = 1)]
            public String B { get; set; }

            [XmlElement(Order = 1)]
            public String C { get; set; }

            [XmlElement(Order = 2)]
            public String D { get; set; }

            [XmlText]
            public String Foo { get; set; }
        }

        [TestMethod]
        public void SerializableStruct() {
            var result = RoundTrip<TestStruct>(
                new TestStruct {
                    Foo = 42,
                    Bar = "Asdf"
                }
            );
            Assert.AreEqual(42, result.Foo);
            Assert.AreEqual("Asdf", result.Bar);
        }

        public struct TestStruct {
            private Int32 foo;
            public String Bar;

            public Int32 Foo {
                get { return foo; }
                set { foo = value; }
            }
        }

        [TestMethod]
        public void SerializableStructMember() {
            var result = RoundTrip<TestStructContainer>(
                new TestStructContainer {
                    Name = "Container",
                    StructProperty = new TestStruct {
                        Foo = 42,
                        Bar = "Asdf"
                    },
                    StructField = new TestStruct {
                        Foo = 2701,
                        Bar = "73x37"
                    }
                }
            );

            Assert.AreEqual("Container", result.Name);
            Assert.AreEqual(42, result.StructProperty.Foo);
            Assert.AreEqual("Asdf", result.StructProperty.Bar);
            Assert.AreEqual(2701, result.StructField.Foo);
            Assert.AreEqual("73x37", result.StructField.Bar);
        }

        public class TestStructContainer {
            public String Name { get; set; }
            public TestStruct StructProperty { get; set; }
            public TestStruct StructField;
        }

        [TestMethod]
        public void PrimitiveRoot() {
            var result = RoundTrip<Int32>(42);
            Assert.AreEqual(42, result);
        }

        [TestMethod]
        public void CollectionRoundtrip() {
            var container = new TestCollectionSerializable {
                SimpleCollection = new List<String> { "Foo", null, "Bar", "Baz" },
                ArrayTest = new Int32[] { 1, 1, 2, 3, 5, 8 },
                UnwrappedList = new List<CollectionChild> { new CollectionChild { Foo = "42", Bar = "Lorem" }, null, new CollectionChild { Foo = "Fhgwah", Bar = "Gds" } },
                XmlArrayTest = new List<Object> { "Asdf", 34, DateTime.Now, new CollectionChild { Foo = "One", Bar = "Two" } },
                BaseTypedList = new List<Base> { new SubOne { JustOne = "One", Another = "AnotherOne" }, null, new SubTwo { JustOne = "Two", YetAnother = "AnotherTwo" } },
            };
            container.CustomCollection.Add("Asdf");
            container.CustomCollection.Add("Qwert");
            container.CustomCollection.Add("One");
            container.CustomCollection.Add("12345");

            var copy = RoundTrip(container);
            CollectionAssert.AreEqual(container.SimpleCollection, copy.SimpleCollection);
            CollectionAssert.AreEqual(container.CustomCollection.ToList(), copy.CustomCollection.ToList());
            CollectionAssert.AreEqual(container.ArrayTest, copy.ArrayTest);
            CollectionAssert.AreEqual(container.XmlArrayTest, copy.XmlArrayTest);
        }

        #region Collection Test

        [XmlRoot("Child")]
        public class CollectionChild : XmlSerializable<CollectionChild> {
            public String Foo { get; set; }
            public String Bar { get; set; }

            public override Boolean Equals(Object obj) {
                var other = obj as CollectionChild;
                return !Object.ReferenceEquals(other, null) && this.Foo == other.Foo && this.Bar == other.Bar;
            }

            public override Int32 GetHashCode() {
                return HashCode.From(typeof(CollectionChild), this.Foo, this.Bar);
            }
        }

        [XmlRoot("TestCollections")]
        public class TestCollectionSerializable : XmlSerializable<TestCollectionSerializable> {
            public List<String> SimpleCollection { get; set; }

            private MySpecialCollection customCollection = new MySpecialCollection();
            [XmlElement]
            public IList<String> CustomCollection { get { return this.customCollection; } }

            public Int32[] ArrayTest { get; set; }

            [XmlElement("Child")]
            public List<CollectionChild> UnwrappedList { get; set; }

            [XmlArray]
            [XmlArrayItem(typeof(Boolean))]
            [XmlArrayItem(typeof(Char))]
            [XmlArrayItem(typeof(SByte))]
            [XmlArrayItem(typeof(Byte))]
            [XmlArrayItem(typeof(Int16))]
            [XmlArrayItem(typeof(UInt16))]
            [XmlArrayItem(typeof(Int32))]
            [XmlArrayItem(typeof(UInt32))]
            [XmlArrayItem(typeof(Int64))]
            [XmlArrayItem(typeof(UInt64))]
            [XmlArrayItem(typeof(Single))]
            [XmlArrayItem(typeof(Double))]
            [XmlArrayItem(typeof(Decimal))]
            [XmlArrayItem(typeof(DateTime))]
            [XmlArrayItem(typeof(DateTimeOffset))]
            [XmlArrayItem(typeof(Guid))]
            [XmlArrayItem(typeof(TimeSpan))]
            [XmlArrayItem(typeof(String))]
            [XmlArrayItem(typeof(char[]))]
            [XmlArrayItem(typeof(byte[]))]
            [XmlArrayItem(typeof(Version))]
            [XmlArrayItem(typeof(List<Int32>))]
            public List<Object> AllThePrimitives { get; set; }

            [XmlArray("MySemiTypedList")]
            [XmlArrayItem("MyString", Type = typeof(String))]
            [XmlArrayItem("MyInt", Type = typeof(Int32))]
            [XmlArrayItem("MyDate", Type = typeof(DateTime))]
            [XmlArrayItem("MyChild", Type = typeof(CollectionChild))]
            public List<Object> XmlArrayTest { get; set; }

            public List<Base> BaseTypedList { get; set; }
        }

        #endregion

        [TestMethod]
        public void CollectionBackwardsCompat() {
            var legacy = new TestCollectionLegacy {
                SimpleCollection = new List<String> { "Foo", null, "Bar", "Baz" },
                CustomCollection = new MySpecialCollection { "Asdf", "Qwert", "One", "12345" },
                ArrayTest = new Int32[] { 1, 1, 2, 3, 5, 8 },
                UnwrappedList = new List<LegacyCollectionChild> { new LegacyCollectionChild { Foo = "42", Bar = "Lorem" }, new LegacyCollectionChild { Foo = "Fhgwah", Bar = "Gds" } },
                XmlArrayTest = new List<Object> { "Asdf", 34, DateTime.Now, new LegacyCollectionChild { Foo = "One", Bar = "Two" } },
                BaseTypedList = new List<Base> { new SubOne { JustOne = "One", Another = "AnotherOne" }, null, new SubTwo { JustOne = "Two", YetAnother = "AnotherTwo" } },
                AllThePrimitives = new List<Object> {
                    true,
                    'a',
                    (Byte)23,
                    (SByte)(-32),
                    (Int16)(-135),
                    (UInt16)531,
                    -91235,
                    31337u,
                    -600000000L,
                    400000000ul,
                    3.1415f,
                    6.28318d,
                    2.7182818284590452353602874713527m,
                    DateTime.Now,
                    Guid.NewGuid(),
                    "asdf",
                    new Char[] { 'q', 'w', 'e', 'r', 't', 'y' },
                    new List<Int32> { 1, 1, 2, 3, 5 },
                    new Byte[] { 65, 83, 68, 70 },
                }
            };
            var newHotness = BackwardCompatRoundTrip<TestCollectionLegacy, TestCollectionSerializable>(legacy);
            CompareCollectionClasses(legacy, newHotness);

            newHotness = BackwardCompatRoundTrip<TestCollectionLegacy, TestCollectionSerializable>(legacy, useLegacySerializer: false);
            CompareCollectionClasses(legacy, newHotness);
        }

        private static void CompareCollectionClasses(TestCollectionLegacy legacy, TestCollectionSerializable newHotness) {
            CollectionAssert.AreEqual(legacy.SimpleCollection, newHotness.SimpleCollection);
            CollectionAssert.AreEqual(legacy.CustomCollection, newHotness.CustomCollection.ToList());
            CollectionAssert.AreEqual(legacy.ArrayTest, newHotness.ArrayTest);
            CollectionAssert.AreEqual(
                legacy.UnwrappedList,
                newHotness.UnwrappedList.Select(obj => new LegacyCollectionChild { Foo = ((CollectionChild)obj).Foo, Bar = ((CollectionChild)obj).Bar }).ToList()
            );
            Assert.AreEqual(
                legacy.AllThePrimitives.Count,
                newHotness.AllThePrimitives.Count
            );
            foreach (var pair in legacy.AllThePrimitives.Zip(newHotness.AllThePrimitives, Tuple.Create)) {
                Assert.AreEqual(pair.Item1.GetType(), pair.Item2.GetType());

                if (typeof(IEnumerable).IsAssignableFrom(pair.Item1.GetType())) {
                    Assert.IsTrue(
                        EnumerableEqualityComparer.Default.Equals(
                            pair.Item1,
                            pair.Item2
                        )
                    );
                } else {
                    Assert.AreEqual(pair.Item1, pair.Item2);
                }
            }
            CollectionAssert.AreEqual(
                legacy.XmlArrayTest,
                newHotness.XmlArrayTest.Select(obj => obj is CollectionChild ? new LegacyCollectionChild { Foo = ((CollectionChild)obj).Foo, Bar = ((CollectionChild)obj).Bar } : obj).ToList()
            );
        }

        #region Legacy Collection Types

        [XmlRoot("Child")]
        public class LegacyCollectionChild {
            public String Foo { get; set; }
            public String Bar { get; set; }

            public override Boolean Equals(Object obj) {
                var other = obj as LegacyCollectionChild;
                return !Object.ReferenceEquals(other, null) && this.Foo == other.Foo && this.Bar == other.Bar;
            }

            public override Int32 GetHashCode() {
                return HashCode.From(typeof(LegacyCollectionChild), this.Foo, this.Bar);
            }
        }

        [XmlInclude(typeof(SubOne))]
        [XmlInclude(typeof(SubTwo))]
        public class Base {
            public String JustOne { get; set; }
        }

        public class SubOne : Base {
            public String Another { get; set; }
        }

        public class SubTwo : Base {
            public String YetAnother { get; set; }
        }

        [XmlRoot("TestCollections")]
        public class TestCollectionLegacy {
            public List<String> SimpleCollection { get; set; }

            [XmlElement]
            public MySpecialCollection CustomCollection { get; set; }

            public Int32[] ArrayTest { get; set; }

            [XmlElement("Child")]
            public List<LegacyCollectionChild> UnwrappedList { get; set; }

            [XmlArray]
            [XmlArrayItem(typeof(Boolean))]
            [XmlArrayItem(typeof(Char))]
            [XmlArrayItem(typeof(SByte))]
            [XmlArrayItem(typeof(Byte))]
            [XmlArrayItem(typeof(Int16))]
            [XmlArrayItem(typeof(UInt16))]
            [XmlArrayItem(typeof(Int32))]
            [XmlArrayItem(typeof(UInt32))]
            [XmlArrayItem(typeof(Int64))]
            [XmlArrayItem(typeof(UInt64))]
            [XmlArrayItem(typeof(Single))]
            [XmlArrayItem(typeof(Double))]
            [XmlArrayItem(typeof(Decimal))]
            [XmlArrayItem(typeof(DateTime))]
            [XmlArrayItem(typeof(DateTimeOffset))]
            [XmlArrayItem(typeof(Guid))]
            [XmlArrayItem(typeof(TimeSpan))]
            [XmlArrayItem(typeof(String))]
            [XmlArrayItem(typeof(char[]))]
            [XmlArrayItem(typeof(byte[]))]
            [XmlArrayItem(typeof(Version))]
            [XmlArrayItem(typeof(List<Int32>))]
            public List<Object> AllThePrimitives { get; set; }

            [XmlArray("MySemiTypedList", IsNullable = true)]
            [XmlArrayItem("MyString", Type = typeof(String))]
            [XmlArrayItem("MyInt", Type = typeof(Int32))]
            [XmlArrayItem("MyDate", Type = typeof(DateTime))]
            [XmlArrayItem("MyChild", Type = typeof(LegacyCollectionChild))]
            public List<Object> XmlArrayTest { get; set; }

            public List<Base> BaseTypedList { get; set; }
        }

        #endregion

        #region MySpecialCollection

        public class MySpecialCollection : IList<String>, ICollection<String>, System.Collections.ICollection, System.Collections.IList {
            private class Node {
                public Int32 Count { get; private set; }
                public String Value { get; private set; }
                public Node Left { get; private set; }
                public Node Right { get; private set; }

                public Node(String value, Node left, Node right) {
                    this.Value = value;
                    this.Left = left;
                    this.Right = right;
                    this.Count = 1 + (left != null ? left.Count : 0) + (right != null ? right.Count : 0);
                }
            }

            private Node root;

            public Int32 IndexOf(String item) {
                var index = 0;
                var found = false;
                Visit(
                    this.root,
                    node => {
                        var cmp = String.Compare(item, node.Value);
                        if (cmp == 0) {
                            found = true;
                            index += node.Left != null ? node.Left.Count : 0;
                            return VisitResult.Done();
                        } else {
                            if (cmp > 0) {
                                // If we're traversing right, then everything to the left and
                                // the current node comes before the item
                                index += node.Left != null ? node.Left.Count + 1 : 1;
                            }
                            return new VisitResult {
                                VisitLeft = cmp < 0,
                                VisitRight = cmp > 0
                            };
                        }
                    }
                );
                return found ? index : ~index;
            }

            public void Insert(Int32 index, String item) {
                throw new NotSupportedException("Insertion is not supported by MySpecialCollection");
            }

            public void RemoveAt(Int32 index) {
                if (index < 0 || index > this.Count) {
                    throw new IndexOutOfRangeException();
                }
                var currentIndex = this.root.Left != null ? this.root.Left.Count : 0;
                this.root = Visit(
                    this.root,
                    node => {
                        if (index == currentIndex) {
                            // here comes the fun part, we need to merge the two children
                            return VisitResult.Done((left, right) => Merge(left, right));
                        } else if (index < currentIndex) {
                            // node.Left must be non-null at this point.

                            // traverse left, decrement the current index by 1 plus the
                            // number of nodes inbetween this node and its left child.
                            currentIndex -= node.Left.Right != null ? node.Left.Right.Count + 1 : 1;
                            return VisitResult.TraverseLeftAndCopy(node.Value);
                        } else {
                            // node.Right must be non-null at this point.

                            // traverse right, increment the current index by 1 plus the
                            // number of nodes inbetween this node and its right child.
                            currentIndex += node.Right.Left != null ? node.Right.Left.Count + 1 : 1;
                            return VisitResult.TraverseRightAndCopy(node.Value);
                        }
                    }
                );
            }

            private Node Merge(Node left, Node right) {
                if (left == null) {
                    if (right == null) {
                        return null;
                    } else {
                        return right;
                    }
                } else if (right == null) {
                    return left;
                } else if (left.Count >= right.Count) {
                    // Take the right most leaf of the left child and make it the new root
                    String newRoot = null;
                    var newLeft = Visit(
                        left,
                        node => {
                            if (node.Right == null) {
                                newRoot = node.Value;
                                return VisitResult.Done((l, r) => l);
                            } else {
                                return VisitResult.TraverseRightAndCopy(node.Value);
                            }
                        }
                    );
                    return new Node(newRoot, newLeft, right);
                } else {
                    // Take the left most leaf of the right child and make it the new root
                    String newRoot = null;
                    var newRight = Visit(
                        right,
                        node => {
                            if (node.Left == null) {
                                newRoot = node.Value;
                                return VisitResult.Done((l, r) => r);
                            } else {
                                return VisitResult.TraverseLeftAndCopy(node.Value);
                            }
                        }
                    );
                    return new Node(newRoot, left, newRight);
                }
            }

            public String this[Int32 index] {
                get {
                    if (index < 0 || index > this.Count) {
                        throw new IndexOutOfRangeException();
                    }
                    Node result = null;
                    var currentIndex = this.root.Left != null ? this.root.Left.Count : 0;
                    Visit(
                        this.root,
                        node => {
                            if (index == currentIndex) {
                                result = node;
                                return VisitResult.Done();
                            } else if (index < currentIndex) {
                                // node.Left must be non-null at this point.

                                // traverse left, decrement the current index by 1 plus the
                                // number of nodes inbetween this node and its left child.
                                currentIndex -= node.Left.Right != null ? node.Left.Right.Count + 1 : 1;
                                return VisitResult.TraverseLeft();
                            } else {
                                // node.Right must be non-null at this point.

                                // traverse right, increment the current index by 1 plus the
                                // number of nodes inbetween this node and its right child.
                                currentIndex += node.Right.Left != null ? node.Right.Left.Count + 1 : 1;
                                return VisitResult.TraverseRight();
                            }
                        }
                    );
                    return result.Value;
                }
                set {
                    throw new NotSupportedException("Assignment by index is not supported by MySpecialCollection");
                }
            }

            public void Add(String item) {
                if (this.root == null) {
                    this.root = new Node(item, null, null);
                } else {
                    this.root =
                        Visit(
                            this.root,
                            node => {
                                var cmp = item.CompareTo(node.Value);
                                if (cmp == 0) {
                                    // This collection is a set, so just ignore duplicate elements
                                    return VisitResult.Done((n1, n2) => node);
                                } else if (cmp < 0) {
                                    if (node.Left == null) {
                                        return VisitResult.Done((left, right) => new Node(node.Value, new Node(item, null, null), right));
                                    } else {
                                        return VisitResult.TraverseLeftAndCopy(node.Value);
                                    }
                                } else {
                                    if (node.Right == null) {
                                        return VisitResult.Done((left, right) => new Node(node.Value, left, new Node(item, null, null)));
                                    } else {
                                        return VisitResult.TraverseRightAndCopy(node.Value);
                                    }
                                }
                            }
                        );
                }
            }

            public void Clear() {
                this.root = null;
            }

            public Boolean Contains(String item) {
                var found = false;
                Search(
                    this.root,
                    item,
                    node => { found = true; }
                );
                return found;
            }

            public void CopyTo(String[] array, Int32 arrayIndex) {
                foreach (var str in this) {
                    if (arrayIndex >= array.Length) break;

                    array[arrayIndex++] = str;
                }
            }

            public Int32 Count {
                get { return this.root != null ? this.root.Count : 0; }
            }

            public Boolean IsReadOnly {
                get { return false; }
            }

            public Boolean Remove(String item) {
                if (this.root == null) {
                    return false;
                } else {
                    var found = false;
                    this.root = SearchWithCopy(
                        this.root,
                        item,
                        node => {
                            // We found the node to remove, replace it with a merged of it's children
                            found = true;
                            return VisitResult.Done((left, right) => Merge(left, right));
                        }
                    );
                    return found;
                }
            }

            public IEnumerator<String> GetEnumerator() {
                return new TreeEnumerator(this.root);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }

            #region Visit

            private class VisitResult {
                public Boolean VisitLeft { get; set; }
                public Boolean VisitRight { get; set; }
                /// <summary>
                /// When set, causes the visitor to make a copy of the tree
                /// </summary>
                public Func<Node, Node, Node> NodeConstructor { get; set; }

                public VisitResult() {
                    this.VisitLeft = true;
                    this.VisitRight = true;
                }

                public static VisitResult Done(Func<Node, Node, Node> nodeConstructor = null) {
                    return new VisitResult {
                        NodeConstructor = nodeConstructor,
                        VisitLeft = false,
                        VisitRight = false,
                    };
                }

                public static VisitResult TraverseLeft(Func<Node, Node, Node> nodeConstructor = null) {
                    return new VisitResult {
                        NodeConstructor = nodeConstructor,
                        VisitRight = false,
                    };
                }

                public static VisitResult TraverseRight(Func<Node, Node, Node> nodeConstructor = null) {
                    return new VisitResult {
                        NodeConstructor = nodeConstructor,
                        VisitLeft = false,
                    };
                }

                public static VisitResult DoneWithCopy(String value) {
                    return Done((l, r) => new Node(value, l, r));
                }

                public static VisitResult TraverseLeftAndCopy(String value) {
                    return TraverseLeft((l, r) => new Node(value, l, r));
                }

                public static VisitResult TraverseRightAndCopy(String value) {
                    return TraverseRight((l, r) => new Node(value, l, r));
                }
            }

            private class VisitState {
                public Node Node { get; private set; }
                public VisitResult Result { get; private set; }
                public Node LeftResult { get; private set; }
                public Boolean LeftResultSet { get; private set; }
                public Node RightResult { get; private set; }
                public Boolean RightResultSet { get; private set; }

                public VisitState(Node original, VisitResult result) {
                    this.Node = original;
                    this.Result = result;
                    this.LeftResult = result.VisitLeft ? null : original.Left;
                    this.LeftResultSet = !result.VisitLeft;
                    this.RightResult = result.VisitRight ? null : original.Right;
                    this.RightResultSet = !result.VisitRight;
                }

                private VisitState(VisitState state) {
                    this.Node = state.Node;
                    this.Result = state.Result;
                    this.LeftResult = state.LeftResult;
                    this.LeftResultSet = state.LeftResultSet;
                    this.RightResult = state.RightResult;
                    this.RightResultSet = state.RightResultSet;
                }

                public VisitState WithLeftResult(Node left) {
                    return new VisitState(this) {
                        LeftResult = left,
                        LeftResultSet = true
                    };
                }

                public VisitState WithRightResult(Node right) {
                    return new VisitState(this) {
                        RightResult = right,
                        RightResultSet = true
                    };
                }
            }

            /// <summary>
            /// Pre-order Traversal w/ Copying
            /// </summary>
            /// <param name="visit"></param>
            private static Node Visit(Node root, Func<Node, VisitResult> visit) {
                var history = new Stack<VisitState>();
                var result = visit(root);
                history.Push(
                    new VisitState(root, result)
                );
                Node finalResult = null;
                while (history.Any()) {
                    var current = history.Peek();
                    Node nextNode = null;
                    if (!current.LeftResultSet && current.Node.Left != null) {
                        // Leave it on the history and process the left
                        nextNode = current.Node.Left;
                    } else if (!current.RightResultSet && current.Node.Right != null) {
                        // Leave it on the history and process the right
                        nextNode = current.Node.Right;
                    } else {
                        // This one's done
                        var newNode =
                            current.Result.NodeConstructor != null ?
                                current.Result.NodeConstructor(current.LeftResult, current.RightResult) :
                                current.Node;
                        history.Pop();
                        if (history.Any()) {
                            var parent = history.Pop();
                            // we always process the left first, if there is anything to process
                            if (!parent.LeftResultSet) {
                                parent = parent.WithLeftResult(newNode);
                            } else {
                                parent = parent.WithRightResult(newNode);
                            }
                            history.Push(parent);
                            // Leave the parent on the stack, we'll see if it's done next iteration
                        } else {
                            finalResult = newNode;
                        }
                    }

                    if (nextNode != null) {
                        var nextResult = visit(nextNode);
                        history.Push(
                            new VisitState(
                                nextNode,
                                nextResult
                            )
                        );
                    }
                }

                return finalResult;
            }

            private static void Search(Node root, String item, Action<Node> found) {
                Visit(
                    root,
                    node => {
                        var cmp = item.CompareTo(node.Value);
                        if (cmp == 0) {
                            // We found the node
                            found(node);
                            return VisitResult.Done();
                        } else if (cmp < 0) {
                            return VisitResult.TraverseLeft();
                        } else {
                            return VisitResult.TraverseRight();
                        }
                    }
                );
            }

            private static Node SearchWithCopy(Node root, String item, Func<Node, VisitResult> found) {
                return Visit(
                    root,
                    node => {
                        var cmp = item.CompareTo(node.Value);
                        if (cmp == 0) {
                            // We found the node
                            return found(node);
                        } else if (cmp < 0) {
                            return VisitResult.TraverseLeftAndCopy(node.Value);
                        } else {
                            return VisitResult.TraverseRightAndCopy(node.Value);
                        }
                    }
                );
            }

            #endregion

            private class TreeEnumerator : IEnumerator<String> {
                private Stack<Node> history = new Stack<Node>();
                private Node root;
                private Node currentNode;
                private Boolean done;

                public TreeEnumerator(Node root) {
                    this.root = root;
                }

                public string Current {
                    get { return this.currentNode != null ? this.currentNode.Value : null; }
                }

                object System.Collections.IEnumerator.Current {
                    get { return this.Current; }
                }

                public bool MoveNext() {
                    if (!this.done) {
                        if (this.currentNode == null) {
                            this.currentNode = this.root;
                            while (this.currentNode != null && this.currentNode.Left != null) {
                                this.history.Push(this.currentNode);
                                this.currentNode = this.currentNode.Left;
                            }
                        } else {
                            if (this.currentNode.Right != null) {
                                // we don't need to preserve this instance of current, since we've already returned everything to the left
                                this.currentNode = this.currentNode.Right;
                                while (this.currentNode != null && this.currentNode.Left != null) {
                                    this.history.Push(this.currentNode);
                                    this.currentNode = this.currentNode.Left;
                                }
                            } else {
                                this.currentNode = this.history.Any() ? this.history.Pop() : null;
                            }
                        }
                    }

                    return this.currentNode != null;
                }

                public void Reset() {
                    this.done = false;
                    this.currentNode = null;
                    this.history.Clear();
                }

                public void Dispose() {
                }
            }

            #region Legacy Collection Bullshit

            int System.Collections.IList.Add(object value) {
                if (value == null || value is String) {
                    var str = (String)value;
                    this.Add(str);
                    // technically we could be more efficient, but meh
                    return this.IndexOf(str);
                } else {
                    throw new ArgumentException("This collection only accepts elements of type string");
                }
            }

            bool System.Collections.IList.Contains(object value) {
                if (value == null || value is String) {
                    return this.Contains((String)value);
                } else {
                    return false;
                }
            }

            int System.Collections.IList.IndexOf(object value) {
                if (value == null || value is String) {
                    return this.IndexOf((String)value);
                } else {
                    throw new ArgumentException("This collection only supports elements of type string");
                }
            }

            void System.Collections.IList.Insert(int index, object value) {
                throw new NotSupportedException();
            }

            bool System.Collections.IList.IsFixedSize {
                get { return false; }
            }

            void System.Collections.IList.Remove(object value) {
                if (value == null || value is String) {
                    this.Remove((String)value);
                } else {
                    throw new ArgumentException("This collection only supports elements of type string");
                }
            }

            object System.Collections.IList.this[int index] {
                get {
                    return this[index];
                }
                set {
                    throw new NotSupportedException();
                }
            }

            void System.Collections.ICollection.CopyTo(Array array, int index) {
                foreach (var str in this) {
                    if (index >= array.Length) break;

                    array.SetValue(str, index++);
                }
            }

            Boolean System.Collections.ICollection.IsSynchronized {
                get { return false; }
            }

            private Object syncRoot = new Object();
            Object System.Collections.ICollection.SyncRoot {
                get { return this.syncRoot; }
            }

            #endregion
        }

        #endregion

        [TestMethod]
        public void SelfReferencingTypeTest() {
            var container = new ContainerTest();
            container.Add("Foo");
            container.Add("Bar");
            container.Add("Baz");
            var result = RoundTrip<ContainerTest>(container);
            Assert.AreEqual("Foo", result.Head.Value);
            Assert.AreEqual("Bar", result.Head.Next.Value);
            Assert.AreEqual("Baz", result.Head.Next.Next.Value);
            Assert.IsNull(result.Head.Next.Next.Next);
        }

        public class ContainerTest : XmlSerializable<ContainerTest> {
            public ItemTest Head { get; set; }

            public void Add(String item) {
                var newItem = new ItemTest { Value = item, Container = this };
                if (this.Head == null) {
                    this.Head = newItem;
                } else {
                    var current = this.Head;
                    while (current.Next != null) current = current.Next;
                    current.Next = newItem;
                }
            }
        }

        public class ItemTest : XmlSerializable<ItemTest> {
            public String Value { get; set; }
            public ItemTest Next { get; set; }
            [XmlIgnore]
            public ContainerTest Container { get; set; }
        }
    }
}
