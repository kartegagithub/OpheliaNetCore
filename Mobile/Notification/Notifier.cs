namespace Ophelia.Mobile.Notification
{
    public abstract class Notifier
    {
        public static Notifier CreateInstance(OperatingSystem OS)
        {
            if (OS == OperatingSystem.iOS)
                return new iOS.Notifier();
            else if (OS == OperatingSystem.Android)
                return new Android.Notifier();

            return null;
        }
    }
}