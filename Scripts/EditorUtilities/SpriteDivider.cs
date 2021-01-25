using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

public struct TextureDividerJob : IJob
{
    public int _startX;
    public int _startY;
    public int _sourceWidth;
    public int _sourceHeight;
    public int _size;
    public NativeArray<Color32> _source;
    public NativeArray<Color32> result;

    public void Execute()
    {
        int index = 0;
        int sourceIndex = _startX + _startY * _sourceWidth;
        Debug.Log(result.Length + " " + _source.Length + " " + _source.Length / _sourceWidth + " " + result.Length / _size);
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                try
                {
                    result[index] = _source[sourceIndex];
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    Debug.Log(index + " " + sourceIndex);
                    break;
                }
                Debug.Log(result[index]);
                index++;
                sourceIndex++;
            }
            Debug.Log(result[index]);
            //sourceIndex--;
            sourceIndex += _sourceWidth - _size;
        }
        //Debug.Log(result.Length + " " + _source.Length);
        //for(int i = 0; i < result.Length; i++)
        //{
        //    result[i] = _source[i];
        //}
    }
}

public class SpriteDivider : MonoBehaviour {

    public int size;

    [HideInInspector]
    public int actual;
    [HideInInspector]
    public int target;

    private Texture2D source;
    Sprite _sprite;

    public void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>().sprite;
        source = _sprite.texture;
    }

    // Use this for initialization
    void Start()
    {
        
    }

    public void Clear()
    {
        int count = transform.childCount;
        for(int i = 0; i < count; i++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        actual = target = 0;
    }

    public void StartDivide()
    {
        StartCoroutine(Divide());
    }

    public IEnumerator Divide()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        _sprite = renderer.sprite;
        source = _sprite.texture;
        actual = 0;
        //Debug.Log(_sprite.rect.width);
        int columns = source.height / size;
        int rows = source.width / size;
        target = columns * rows;
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                //Texture2D newTex = new Texture2D(size, size);
                //NativeArray<Color32> sourceArray = new NativeArray<Color32>(source.GetPixels32(), Allocator.TempJob);
                //NativeArray<Color32> newTexArray = new NativeArray<Color32>(newTex.GetPixels32(), Allocator.TempJob);
                //TextureDividerJob job = new TextureDividerJob
                //{
                //    _startX = i*size,
                //    _startY = j*size,
                //    _size = size,
                //    _sourceWidth = source.width,
                //    _sourceHeight = source.height,
                //    _source = sourceArray,
                //    result = newTexArray
                //};
                //JobHandle handle = job.Schedule();
                //handle.Complete();
                //Color32[] pixels = new Color32[newTexArray.Length];
                //newTexArray.CopyTo(pixels);

                //newTex.SetPixels32(pixels);
                //newTex.Apply();
                Sprite newSprite = Sprite.Create(source, new Rect(i * size, j * size, size, size), new Vector2(0.5f, 0.5f), _sprite.pixelsPerUnit);
                //Sprite newSprite = Sprite.Create(newTex, new Rect(0,0, size,size),new Vector2(0.5f,0.5f));
                GameObject n = new GameObject("(" + i + "," + j + ")");
                SpriteRenderer sr = n.AddComponent<SpriteRenderer>();
                sr.sprite = newSprite;
                sr.color = renderer.color;
                n.transform.parent = transform;
                n.transform.localScale = Vector3.one;
                float horizontalCompensation = -(_sprite.pivot.x / _sprite.pixelsPerUnit) + (newSprite.pivot.x / _sprite.pixelsPerUnit);
                float verticalCompensation = -(_sprite.pivot.y / _sprite.pixelsPerUnit) + (newSprite.pivot.y / _sprite.pixelsPerUnit);
                n.transform.localRotation = Quaternion.identity;
                n.transform.localPosition = new Vector3(i * size / _sprite.pixelsPerUnit + (horizontalCompensation), j * size / _sprite.pixelsPerUnit + (verticalCompensation), 0);
                actual++;
                //sourceArray.Dispose();
                //newTexArray.Dispose();
                yield return null;
            }
        }
        GetComponent<SpriteRenderer>().enabled = false;
    }
}
