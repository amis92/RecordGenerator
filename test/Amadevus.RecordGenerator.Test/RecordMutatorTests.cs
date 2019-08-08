using System;
using Amadevus.RecordGenerator.TestsBase;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Xunit;

namespace Amadevus.RecordGenerator.Test
{
    public class RecordMutatorTests : RecordTestsBase
    {

        [Fact]
        public void With_SimpleProperty_DoesNotModifyInstance()
        {
            const string newName = "New " + ItemName;
            var item = CreateItem();
            item.WithName(newName);
            Assert.Equal(ItemName, item.Name);
        }

        [Fact]
        public void With_SimpleProperty_CreatesModifiedInstance()
        {
            const string newName = "New " + ItemName;
            var item = CreateItem();
            var modifiedItem = item.WithName(newName);
            Assert.Equal(newName, modifiedItem.Name);
        }

        [Fact]
        public void Ctor_InvokesValidate_Throwing()
        {
            var item = CreateItem();
            Assert.Throws<ArgumentNullException>("value", () => item.WithName(null));
        }

        [Fact]
        public void With_CollectionProperty_DoesNotModifyInstance()
        {
            var item = CreateItem();
            var container = CreateContainerEmpty();
            container.WithItems(new[] { item }.ToImmutableArray());
            Assert.Equal(default(ImmutableArray<Item>), container.Items);
        }

        [Fact]
        public void With_CollectionProperty_CreatesModifiedInstance()
        {
            var item = CreateItem();
            var container = CreateContainerEmpty();
            var modifiedContainer = container.WithItems(new[] { item }.ToImmutableArray());
            Assert.Collection(modifiedContainer.Items, x => Assert.Same(item, x));
        }
    }
}
