using System.Drawing;

namespace hcClient.ui
{
    enum WidgetPosition { Center, CursorRight, CursorTop };

    interface IWidgetContainer
    {
        void AddWidget(WidgetBase widget);
        void RemoveWidget(WidgetBase widget);

        void PopupWidget(WidgetBase widget, WidgetPosition position);
        void ClosePopup();

        void Invalidate(Rectangle rect);

        void DataChanged(int id, int data);
        void ChangeData(int id, int data);
        int GetData(int id);
    }
}
