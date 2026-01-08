using Cysharp.Threading.Tasks;

namespace LiteUI.Notification.Controller
{
    public interface INotification
    {
        UniTask Show();

        UniTask Hide();
    }
}
