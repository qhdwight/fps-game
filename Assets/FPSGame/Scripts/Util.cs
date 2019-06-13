using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

static class Util
{
    public static Vector3 WorldPosToVector3(WorldPosition pos)
    {
        return new Vector3(pos.x, pos.y, pos.z);
    }

    public static WorldPosition Vector3ToWorldPos(Vector3 v3)
    {
        return new WorldPosition(RoundToInt(v3.x), RoundToInt(v3.y), RoundToInt(v3.z));
    }

    public static Quaternion SnapQuaternion(Quaternion quat)
    {
        Vector3 euler = quat.eulerAngles;
        euler.x = 0F; euler.z = 0F;
        euler.y = Mathf.Round(euler.y/90F)*90F;
        return Quaternion.Euler(euler);
    }

    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
            return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null)
                continue;

            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private const string INDENT_STRING = "    ";
    public static string FormatJSON(string str)
    {
        var indent = 0;
        var quoted = false;
        var sb = new StringBuilder();
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            switch (ch)
            {
                case '{':
                case '[':
                    sb.Append(ch);
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                    }
                    break;
                case '}':
                case ']':
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                    }
                    sb.Append(ch);
                    break;
                case '"':
                    sb.Append(ch);
                    bool escaped = false;
                    var index = i;
                    while (index > 0 && str[--index] == '\\')
                        escaped = !escaped;
                    if (!escaped)
                        quoted = !quoted;
                    break;
                case ',':
                    sb.Append(ch);
                    if (!quoted)
                    {
                        sb.AppendLine();
                        Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                    }
                    break;
                case ':':
                    sb.Append(ch);
                    if (!quoted)
                        sb.Append(" ");
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }
        return sb.ToString();
    }

    public static IEnumerator FadeInImage(Image img, float max = 1F)
    {
        img.color = new Color(1F, 1F, 1F, 0F);
        Color col = img.color;
        while (col.a < max)
        {
            col.a += 0.1F;
            img.color = col;
            yield return new WaitForSeconds(0.1F);
        }
    }

    public static float GetDistance(WorldPosition w1, WorldPosition w2) {
        float deltaX = w2.x - w1.x;
        float deltaY = w2.y - w1.y;
        float deltaZ = w2.z - w1.z;
        float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        return distance;
    }

    public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
    {
        foreach (var i in ie)
        {
            action(i);
        }
    }

    public static Vector3 Round(Vector3 vec, float yOffset = 0F)
    {
        return new Vector3(Round(vec.x, 0), Round(vec.y + yOffset, 0), Round(vec.z, 0));
    }

    public static Quaternion Round(Quaternion quat)
    {
        return new Quaternion(Round(quat.x, 1), Round(quat.y, 1), Round(quat.z, 1), Round(quat.w, 1));
    }

    public static float Round(float f, int degree)
    {
        return (float)Math.Round(f, degree);
    }

    public static Ray AddRandomToRay(Ray ray, float min, float max)
    {
        Vector3 dir = new Vector3(
            ray.direction.x + UnityEngine.Random.Range(min, max),
            ray.direction.y + UnityEngine.Random.Range(min, max),
            ray.direction.z + UnityEngine.Random.Range(min, max));
        ray.direction = dir;
        return ray;
    }

    public static int RoundToInt(float f)
    {
        return (int)Math.Round(f, 1);
    }

    public static Color ShiftColor(Color c, float amount = 0.2F)
    {
        c.r += amount; c.g += amount; c.b += amount;
        return c;
    }

    public static Vector2 ScaleVector(Vector2 vector, float maxMagnitude)
    {
        if (vector.magnitude > maxMagnitude && vector.magnitude != 0F)
        {
            vector *= maxMagnitude / vector.magnitude;
        }
        return vector;
    }

    // public static byte[] ObjectToByteArray(object obj)
    // {
    //     if (obj == null)
    //         return null;

    //     BinaryFormatter bf = new BinaryFormatter();
    //     using (MemoryStream ms = new MemoryStream())
    //     {
    //         bf.Serialize(ms, obj);
    //         return ms.ToArray();
    //     }
    // }

    // public static object ByteArrayToObject(byte[] arrBytes)
    // {
    //     MemoryStream memStream = new MemoryStream();
    //     BinaryFormatter binForm = new BinaryFormatter();
    //     memStream.Write(arrBytes, 0, arrBytes.Length);
    //     memStream.Seek(0, SeekOrigin.Begin);
    //     object obj = (object)binForm.Deserialize(memStream);

    //     return obj;
    // }
}