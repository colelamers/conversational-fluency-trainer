using System.Collections.Generic;

namespace core.strucutres.dawg.models;

public class Node {
    public int Id { get; set; }
    public string Value { get; set; }
    public bool IsTerminal { get; set; }
    public bool IsSequenceStart { get; set; }

    public Node(int id, string value) {
        Id = id;
        Value = value;
        IsTerminal = false;
        IsSequenceStart = false;
    }
}
