using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.Annotations;
using System;
using System.Application.UI.Adapters;

// ReSharper disable once CheckNamespace
namespace AndroidX.RecyclerView.Widget
{
    /// <summary>
    /// 列表项的垂直间距
    /// </summary>
    public sealed class VerticalItemDecoration : RecyclerView.ItemDecoration
    {
        readonly int height;

        public VerticalItemDecoration(int height)
        {
            this.height = height;
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            base.GetItemOffsets(outRect, view, parent, state);

            var holder = parent.GetChildViewHolder(view);
            var position = holder.BindingAdapterPosition;
            if (parent.GetAdapter() is IReadOnlyViewModels<object> viewModels)
            {
                if (position < viewModels.ViewModels.Count - 1)
                {
                    // 除了末尾项之外，底部添加相同的间距
                    outRect.Bottom = height;
                }
            }
        }
    }

    /// <inheritdoc cref="VerticalItemDecoration"/>
    public sealed class VerticalItemDecoration2 : RecyclerView.ItemDecoration
    {
        readonly int height;
        readonly int paddingBottom;
        readonly bool noTop;

        public bool PaddingBottomWithHeight { get; set; }

        public VerticalItemDecoration2(int height, int paddingBottom, bool noTop = false)
        {
            this.height = height;
            this.paddingBottom = paddingBottom;
            this.noTop = noTop;
        }

        public static VerticalItemDecoration2 Get(Context context, [DimenRes] int heightResId, [DimenRes] int paddingBottomResId = default, bool noTop = false)
        {
            var height = context.GetDimensionPixelSize(heightResId);
            var paddingBottom = paddingBottomResId != default ? context.GetDimensionPixelSize(paddingBottomResId) : default;
            return new(height, paddingBottom, noTop);
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            base.GetItemOffsets(outRect, view, parent, state);

            var holder = parent.GetChildViewHolder(view);
            var position = holder.BindingAdapterPosition;
            var adapter = parent.GetAdapter();
            var count = adapter.ItemCount;
            if (count == 0)
            {
                return;
            }
            else if (position == 0)
            {
                if (!noTop) outRect.Top = height;
            }
            if (position < count - 1)
            {
                outRect.Bottom = height;
            }
            else
            {
                outRect.Bottom = PaddingBottomWithHeight ? paddingBottom + height : (paddingBottom == 0 ? height : paddingBottom);
            }
        }
    }
}