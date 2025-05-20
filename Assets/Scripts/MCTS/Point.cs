using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Đại diện cho một điểm (tọa độ x, y) trên bàn cờ.
/// Dùng để xác định vị trí các quân cờ, truyền dữ liệu vị trí giữa các hàm.
/// </summary>
public class Point
{
    public int x { get; set; }
    public int y { get; set; }

    public Point(int newX, int newY)
    {
        x = newX;
        y = newY;
    }

    public bool isEqual(Point p)
    {
        if (p != null)
        {
            return ((this.x == p.x) && (this.y == p.y));
        }
        else
        {
            return false;
        }
    }
}
