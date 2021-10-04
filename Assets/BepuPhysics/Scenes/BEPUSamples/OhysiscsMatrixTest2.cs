using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

public static class PhysicsCollisionMatrixLayerMasks2
{

    private static bool[,] m_CollisionMatrix = new bool[32, 32];

    public static bool[,] CollisionMatrix => m_CollisionMatrix;
    public static void SaveCollisionMatrix(bool saveToFile)
    {
        string strName = "";
        string strID = "";
        for (int i = 0; i < m_CollisionMatrix.GetLength(0); ++i)
        {
            for (int j = 0; j < m_CollisionMatrix.GetLength(1) - i; ++j)
            {
                m_CollisionMatrix[i, j] = !Physics.GetIgnoreLayerCollision(i, j);
                strName += "[" + LayerMask.LayerToName(i) + "/" + LayerMask.LayerToName(j) + "(" + m_CollisionMatrix[i, j] + ")] ";
                strID += "[" + i + "/" + j + "(" + m_CollisionMatrix[i, j] + ")] ";
            }
            strName += "\n";
            strID += "\n";
        }

        if (!saveToFile)
            return;

        FileStream fWrite = new FileStream(Application.dataPath + "/PhysicsMatrix(WithName).txt",
                   FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        byte[] writeArr = Encoding.UTF8.GetBytes(strName);

        fWrite.Write(writeArr, 0, strName.Length);
        fWrite.Close();

        fWrite = new FileStream(Application.dataPath + "/PhysicsMatrix(ID).txt",
                   FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        writeArr = Encoding.UTF8.GetBytes(strID);

        fWrite.Write(writeArr, 0, strID.Length);
        fWrite.Close();
    }

    public static void LoadCollisionMatrix()
    {
        for (int i = 0; i < m_CollisionMatrix.GetLength(0); ++i)
        {
            for (int j = 0; j < m_CollisionMatrix.GetLength(1) - i; ++j)
            {
                Physics.IgnoreLayerCollision(i, j, !m_CollisionMatrix[i, j]);
            }
        }
    }
}
