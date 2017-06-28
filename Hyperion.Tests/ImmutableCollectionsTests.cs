#region copyright
// -----------------------------------------------------------------------
//  <copyright file="ImmutableCollectionsTests.cs" company="Akka.NET Team">
//      Copyright (C) 2015-2016 AsynkronIT <https://github.com/AsynkronIT>
//      Copyright (C) 2016-2016 Akka.NET Team <https://github.com/akkadotnet>
//  </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Hyperion.Tests
{
    public class ImmutableCollectionTests : TestBase
    {
        #region internal classes

        class TestClass<T, U>
        {
            // KeyValuePair<int, int> because Mono has broken structural equality on KeyValuePair<,>
            public IImmutableDictionary<int, T> Dictionary { get; }

            public IImmutableList<U> List { get; }
            public IImmutableQueue<U> Queue { get; }
            public IImmutableSet<U> SortedSet { get; }
            public IImmutableSet<U> HashSet { get; }
            public IImmutableStack<U> Stack { get; }

            public TestClass(IImmutableDictionary<int, T> dictionary, IImmutableList<U> list, IImmutableQueue<U> queue,
                IImmutableSet<U> sortedSet, IImmutableSet<U> hashSet, IImmutableStack<U> stack)
            {
                Dictionary = dictionary;
                List = list;
                Queue = queue;
                SortedSet = sortedSet;
                HashSet = hashSet;
                Stack = stack;
            }

            protected bool Equals(TestClass<T, U> other)
            {
                return SeqEquals(Dictionary, other.Dictionary)
                       && SeqEquals(List, other.List)
                       && SeqEquals(Queue, other.Queue)
                       && SeqEquals(SortedSet, other.SortedSet)
                       && SeqEquals(HashSet, other.HashSet)
                       && SeqEquals(Stack, other.Stack);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TestClass<T, U>) obj);
            }

            private static bool SeqEquals<V>(IEnumerable<V> first, IEnumerable<V> second)
            {
                var f = first.GetEnumerator();
                var s = second.GetEnumerator();
                while (f.MoveNext() && s.MoveNext())
                {
                    var x = f.Current;
                    var y = s.Current;

                    if (!Equals(x, y))
                    {
                        return false;
                    }
                }

                if (f.MoveNext() || s.MoveNext())
                {
                    // collections are not equal in size
                    return false;
                }

                return true;
            }
        }

        interface IContainer<T> : IComparer<IContainer<T>>, IComparable
        {
            T Value { get; }
        }

        class Container<T> : IContainer<T>
        {
            private readonly T value;

            public T Value => value;

            public Container(T value)
            {
                this.value = value;
            }

            public int Compare(IContainer<T> x, IContainer<T> y)
            {
                var comparer = new ContainerComparer<T>(Comparer<T>.Default);
                return comparer.Compare(x, y);
            }

            public int CompareTo(object obj)
            {
                if (obj == null)
                {
                    return 1;
                }

                if (obj is IContainer<T>)
                {
                    return Compare(this, (IContainer<T>) obj);
                }

                return 1;
            }
        }

        class ContainerComparer<T> : IComparer<IContainer<T>>
        {
            private readonly IComparer<T> innerComparer;

            public int Compare(IContainer<T> x, IContainer<T> y)
            {
                if (x == null && y == null)
                {
                    return 0;
                }

                if (x == null)
                {
                    return -1;
                }

                if (y == null)
                {
                    return 1;
                }

                return innerComparer.Compare(x.Value, y.Value);
            }

            public ContainerComparer(IComparer<T> innerComparer)
            {
                this.innerComparer = innerComparer;
            }
        }

        #endregion

        [Fact]
        public void CanSerializeImmutableHashSet()
        {
            var expected = ImmutableHashSet.CreateRange(new[]
            {
                new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                new Something(),
                new Something()
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableHashSet<Something>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
        public void CanSerializeImmutableSortedSet()
        {
            var expected = ImmutableSortedSet.CreateRange(new[]
            {
                "abc",
                "abcd",
                "abcde"
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableSortedSet<string>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
        public void CanSerializeImmutableDictionary()
        {
            var expected = ImmutableDictionary.CreateRange(new Dictionary<string, Something>
            {
                ["a1"] = new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                ["a2"] = new Something(),
                ["a3"] = new Something(),
                ["a4"] = null
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableDictionary<string, Something>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
        public void CanSerializeImmutableQueue()
        {
            var expected = ImmutableQueue.CreateRange(new[]
            {
                new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                new Something(),
                new Something(),
                null
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableQueue<Something>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
        public void CanSerializeImmutableStack()
        {
            var expected = ImmutableStack.CreateRange(new[]
            {
                new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                new Something(),
                new Something()
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableStack<Something>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
        public void CanSerializeImmutableArray()
        {
            var expected = ImmutableArray.CreateRange(new[]
            {
                new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                new Something(),
                new Something(),
                null
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableArray<Something>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
        public void CanSerializeImmutableList()
        {
            var expected = ImmutableList.CreateRange(new[]
            {
                new Something
                {
                    BoolProp = true,
                    Else = new Else
                    {
                        Name = "Yoho"
                    },
                    Int32Prop = 999,
                    StringProp = "Yesbox!"
                },
                new Something(),
                new Something(),
                null
            });

            Serialize(expected);
            Reset();
            var actual = Deserialize<ImmutableList<Something>>();
            Assert.Equal(expected.ToList(), actual.ToList());
        }

        [Fact]
        public void CanSerializeImmutableCollectionsReferencedThroughInterfaceInFields()
        {
            var expected = new TestClass<int, string>(
                dictionary: ImmutableDictionary.CreateRange(new[]
                {
                    new KeyValuePair<int, int>(2, 1),
                    new KeyValuePair<int, int>(3, 2),
                }),
                list: ImmutableList.CreateRange(new[] {"c", "d"}),
                queue: ImmutableQueue.CreateRange(new[] {"e", "f"}),
                sortedSet: ImmutableSortedSet.CreateRange(new[] {"g", "h"}),
                hashSet: ImmutableHashSet.CreateRange(new[] {"i", "j"}),
                stack: ImmutableStack.CreateRange(new[] {"k", "l"}));

            Serialize(expected);
            Reset();
            var actual = Deserialize<TestClass<int, string>>();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanSerializeImmutableCollectionsOfTypesWithGenericParameters()
        {
            var comparer = new ContainerComparer<int?>(Comparer<int?>.Default);
            var expected = new TestClass<IContainer<int?>, IContainer<int?>>(
                dictionary: ImmutableDictionary.CreateRange(new[]
                {
                    new KeyValuePair<int, IContainer<int?>>(2, new Container<int?>(1)),
                    new KeyValuePair<int, IContainer<int?>>(3, new Container<int?>(2)),
                }),
                list: ImmutableList.CreateRange<IContainer<int?>>(new[] {new Container<int?>(3), new Container<int?>(4)}),
                queue: ImmutableQueue.CreateRange<IContainer<int?>>(new[] {new Container<int?>(5), new Container<int?>(6)}),
                sortedSet: ImmutableSortedSet.CreateRange<IContainer<int?>>(comparer, new[] {new Container<int?>(7), new Container<int?>(8)}),
                hashSet: ImmutableHashSet.CreateRange<IContainer<int?>>(new[] {new Container<int?>(9), new Container<int?>(10)}),
                stack: ImmutableStack.CreateRange<IContainer<int?>>(new[] {new Container<int?>(11), new Container<int?>(12)}));

            Serialize(expected);
            Reset();
            var actual = Deserialize<TestClass<IContainer<int?>, IContainer<int?>>>();
            actual.ShouldBeEquivalentTo(expected);
        }
    }
}