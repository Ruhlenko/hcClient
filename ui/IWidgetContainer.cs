using System.Drawing;

namespace hcClient.ui
{
    enum WidgetPosition { Center, CursorRight, CursorTop };

    interface IWidgetContainer
    {
        void AddWidget(Widget widget);
        void RemoveWidget(Widget widget);

        void PopupWidget(Widget widget, WidgetPosition position);
        void ClosePopup();

        void Invalidate(Rectangle rect);

        void DataChanged(int id, int data);
        void ChangeData(int id, int data);
        int GetData(int id);
    }
}
