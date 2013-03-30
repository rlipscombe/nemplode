using System;
using System.Windows;
using System.Windows.Controls;

namespace ViewTags
{
    internal class PrivateFrameColumn : DataGridTextColumn, IEquatable<PrivateFrameColumn>
    {
        private readonly string _owner;

        public PrivateFrameColumn(TagLib.Id3v2.PrivateFrame frame)
        {
            _owner = frame.Owner;

            Header = string.Format("PRIV:{0}", _owner);
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var tag = (TagLib.Id3v2.Tag) dataItem;

            var privateFrame = TagLib.Id3v2.PrivateFrame.Get(tag, _owner, create: false);
            if (privateFrame == null)
                return new TextBlock();

            if (privateFrame.Owner == "WM/WMContentID")
                return new TextBlock { Text = new Guid(privateFrame.PrivateData.Data).ToString() };
            else if (privateFrame.Owner == "WM/WMCollectionID")
                return new TextBlock { Text = new Guid(privateFrame.PrivateData.Data).ToString() };
            else if (privateFrame.Owner == "WM/WMCollectionGroupID")
                return new TextBlock {Text = new Guid(privateFrame.PrivateData.Data).ToString() };
            else
                return new TextBlock {Text = privateFrame.ToString()};
        }

        public bool Equals(PrivateFrameColumn other)
        {
            return _owner.Equals(other._owner);
        }
    }
}