using System;

public class ByteArray
{
    //默认大小
    public const int DEFAULT_SIZE = 1024;
    //初始大小
    private int m_InitSize = 0;
    //缓冲区
    public byte[] Bytes;
    //读写位置, ReadIdx = 开始读的索引，WriteIdx = 已经写入的索引
    public int ReadIdx = 0;
    public int WriteIdx = 0;
    //容量
    private int Capacity = 0;

    //剩余空间
    public int Remain { get { return Capacity - WriteIdx; } }

    //数据长度
    public int Length { get { return WriteIdx - ReadIdx; } }

    public ByteArray()
    {
        Bytes = new byte[DEFAULT_SIZE];
        Capacity = DEFAULT_SIZE;
        m_InitSize = DEFAULT_SIZE;
        ReadIdx = 0;
        WriteIdx = 0;
    }

    public ByteArray(byte[] dafalutBytes) 
    {
        Bytes = dafalutBytes;
        Capacity = dafalutBytes.Length;
        m_InitSize = dafalutBytes.Length;
        ReadIdx = 0;
        WriteIdx = dafalutBytes.Length;
    }

    /// <summary>
    /// 检测并移动数据
    /// </summary>
    public void CheckAndMoveBytes()
    {
        if (Length < 8)
        {
            MoveBytes();
        }
    }

    /// <summary>
    /// 移动数据
    /// </summary>
    public void MoveBytes()
    {
        if (ReadIdx < 0)
            return;

        Array.Copy(Bytes, ReadIdx, Bytes, 0, Length);
        WriteIdx = Length;
        ReadIdx = 0;
    }

    /// <summary>
    /// 重设尺寸
    /// </summary>
    /// <param name="size"></param>
    public void ReSize(int size)
    {
        if (ReadIdx < 0) return;
        if (size < Length) return;
        if (size < m_InitSize) return;
        int n = 1024;
        while (n < size) n *= 2;
        Capacity = n;
        byte[] newBytes = new byte[Capacity];
        Array.Copy(Bytes, ReadIdx, newBytes, 0, Length);
        Bytes = newBytes;
        WriteIdx = Length;
        ReadIdx = 0;
    }
}
