namespace core.algs.models;

public readonly struct 
WordWindow
{
    public List<string> Words { get; }
    public int Offset { get; }
    public int Length { get; }

    public WordWindow(List<string> words, int offset, int length) {
        Words = words;
        Offset = offset;
        Length = length;
    }
}
