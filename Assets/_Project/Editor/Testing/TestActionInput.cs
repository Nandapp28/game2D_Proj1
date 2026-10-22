// Alat uji: IActionInput yang "ditekan" dari kode satu kali (menggantikan tombol E saat uji otomatis).
public class TestActionInput : IActionInput
{
    public static readonly TestActionInput Instance = new TestActionInput();
    private int pending;

    public static void Press() => Instance.pending++;

    public bool InteractPressed
    {
        get
        {
            if (pending <= 0) return false;
            pending--;
            return true;
        }
    }
}
