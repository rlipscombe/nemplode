using System;
using System.Linq;
using System.Windows.Controls;

namespace ViewTags
{
    internal static class FrameColumn
    {
        public static DataGridColumn Create(TagLib.Id3v2.Frame frame)
        {
            if (frame is TagLib.Id3v2.PrivateFrame)
                return new PrivateFrameColumn((TagLib.Id3v2.PrivateFrame)frame);
            else if (frame is TagLib.Id3v2.TextInformationFrame)
                return new TextInformationFrameColumn((TagLib.Id3v2.TextInformationFrame)frame);

            return new DataGridTextColumn();
        }
    }

    internal class TextInformationFrameColumn : DataGridTextColumn, IEquatable<TextInformationFrameColumn>
    {
        private readonly TagLib.ByteVector _ident;

        public TextInformationFrameColumn(TagLib.Id3v2.Frame frame)
        {
            Header = frame.FrameId;
            _ident = frame.FrameId;
        }

        protected override System.Windows.FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var tag = (TagLib.Id3v2.Tag) dataItem;
            var frame = (TagLib.Id3v2.TextInformationFrame) tag.GetFrames(_ident).FirstOrDefault();
            if (frame == null)
                return new TextBlock();
            else
                return new TextBlock {Text = frame.Text[0]};
        }

        public bool Equals(TextInformationFrameColumn other)
        {
            return _ident.Equals(other._ident);
        }
    }
}