namespace LdLib.CompilerTypes;

public struct Dynamic<TYpe1, TYpe2>
{
    private TYpe1? Ype1;
    private TYpe2? Ype2;

    public Dynamic(TYpe1? ype1, TYpe2? ype2)
    {
        if (ype1 == null && ype2 == null) throw new Exception("Both This and That are null");
        if (ype1 != null && ype2 != null) throw new Exception("Both This and That are not null");
        Ype1 = ype1;
        Ype2 = ype2;
    }


    public object TheOne()
    {
        if (Ype1 == null && Ype2 == null) throw new Exception("Both This and That are null");
        if (Ype1 != null && Ype2 != null) throw new Exception("Both This and That are not null");
        if (Ype1 != null) return Ype1;
        if (Ype2 != null) return Ype2;
        throw new Exception("Both This and That are null");
    }
}