using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCollector : MonoBehaviour {

    private Animate<Vector3>[] animatesVector3;
    private Animate<float>[] animatesFloat;
    private Animate<Color>[] animatesColor;

    private void Awake()
    {
        animatesVector3 = GetComponents<Animate<Vector3>>();
        animatesFloat = GetComponents<Animate<float>>();
        animatesColor = GetComponents<Animate<Color>>();
    }

    public void AnimationReset(string _id)
    {
        bool found = false;
        foreach(Animate<Vector3> anim in animatesVector3)
        {
            if (anim.id == _id)
            {
                anim.AnimationReset();
                found = true;
            }
        }
        foreach (Animate<float> anim in animatesFloat)
        {
            if (anim.id == _id)
            {
                anim.AnimationReset();
                found = true;
            }
        }
        foreach (Animate<Color> anim in animatesColor)
        {
            if (anim.id == _id)
            {
                anim.AnimationReset();
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogWarning("No Animate with ID \"" + _id + "\" found on GameObject \"" + gameObject.name + "\"");
        }
    }

    public void AnimationSetEnd(string _id)
    {
        bool found = false;
        foreach (Animate<Vector3> anim in animatesVector3)
        {
            if (anim.id == _id)
            {
                anim.AnimationSetEnd();
                found = true;
            }
        }
        foreach (Animate<float> anim in animatesFloat)
        {
            if (anim.id == _id)
            {
                anim.AnimationSetEnd();
                found = true;
            }
        }
        foreach (Animate<Color> anim in animatesColor)
        {
            if (anim.id == _id)
            {
                anim.AnimationSetEnd();
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogWarning("No Animate with ID \"" + _id + "\" found on GameObject \"" + gameObject.name + "\"");
        }
    }

    public virtual void PlayForward(string _id)
    {
        bool found = false;
        foreach (Animate<Vector3> anim in animatesVector3)
        {
            if (anim.id == _id)
            {
                anim.PlayForward();
                found = true;
            }
        }
        foreach (Animate<float> anim in animatesFloat)
        {
            if (anim.id == _id)
            {
                anim.PlayForward();
                found = true;
            }
        }
        foreach (Animate<Color> anim in animatesColor)
        {
            if (anim.id == _id)
            {
                anim.PlayForward();
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogWarning("No Animate with ID \"" + _id + "\" found on GameObject \"" + gameObject.name + "\"");
        }
    }

    public virtual void PlayAt(float _point, string _id)
    {
        bool found = false;
        foreach (Animate<Vector3> anim in animatesVector3)
        {
            if (anim.id == _id)
            {
                anim.PlayAt(_point);
                found = true;
            }
        }
        foreach (Animate<float> anim in animatesFloat)
        {
            if (anim.id == _id)
            {
                anim.PlayAt(_point);
                found = true;
            }
        }
        foreach (Animate<Color> anim in animatesColor)
        {
            if (anim.id == _id)
            {
                anim.PlayAt(_point);
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogWarning("No Animate with ID \"" + _id + "\" found on GameObject \"" + gameObject.name + "\"");
        }
    }

    public virtual float GetLerp(string _id)
    {
        bool found = false;
        float returnable = 0;
        foreach (Animate<Vector3> anim in animatesVector3)
        {
            if (anim.id == _id)
            {
                returnable = anim.ActualLerp;
                found = true;
            }
        }
        foreach (Animate<float> anim in animatesFloat)
        {
            if (anim.id == _id)
            {
                returnable = anim.ActualLerp;
                found = true;
            }
        }
        foreach (Animate<Color> anim in animatesColor)
        {
            if (anim.id == _id)
            {
                returnable = anim.ActualLerp;
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogWarning("No Animate with ID \"" + _id + "\" found on GameObject \"" + gameObject.name + "\"");
        }
        return returnable;
    }

    public virtual void PlayBackward(string _id)
    {
        bool found = false;
        foreach (Animate<Vector3> anim in animatesVector3)
        {
            if (anim.id == _id)
            {
                anim.PlayBackward();
                found = true;
            }
        }
        foreach (Animate<float> anim in animatesFloat)
        {
            if (anim.id == _id)
            {
                anim.PlayBackward();
                found = true;
            }
        }
        foreach (Animate<Color> anim in animatesColor)
        {
            if (anim.id == _id)
            {
                anim.PlayBackward();
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogWarning("No Animate with ID \"" + _id + "\" found on GameObject \"" + gameObject.name + "\"");
        }
    }

    public void Stop(string _id)
    {
        bool found = false;
        foreach (Animate<Vector3> anim in animatesVector3)
        {
            if (anim.id == _id)
            {
                anim.Stop();
                found = true;
            }
        }
        foreach (Animate<float> anim in animatesFloat)
        {
            if (anim.id == _id)
            {
                anim.Stop();
                found = true;
            }
        }
        foreach (Animate<Color> anim in animatesColor)
        {
            if (anim.id == _id)
            {
                anim.Stop();
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogWarning("No Animate with ID \"" + _id + "\" found on GameObject \"" + gameObject.name + "\"");
        }
    }

    public void Pause(string _id)
    {
        bool found = false;
        foreach (Animate<Vector3> anim in animatesVector3)
        {
            if (anim.id == _id)
            {
                anim.Pause();
                found = true;
            }
        }
        foreach (Animate<float> anim in animatesFloat)
        {
            if (anim.id == _id)
            {
                anim.Pause();
                found = true;
            }
        }
        foreach (Animate<Color> anim in animatesColor)
        {
            if (anim.id == _id)
            {
                anim.Pause();
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogWarning("No Animate with ID \"" + _id + "\" found on GameObject \"" + gameObject.name + "\"");
        }
    }

    public void Resume(string _id)
    {
        bool found = false;
        foreach (Animate<Vector3> anim in animatesVector3)
        {
            if (anim.id == _id)
            {
                anim.Resume();
                found = true;
            }
        }
        foreach (Animate<float> anim in animatesFloat)
        {
            if (anim.id == _id)
            {
                anim.Resume();
                found = true;
            }
        }
        foreach (Animate<Color> anim in animatesColor)
        {
            if (anim.id == _id)
            {
                anim.Resume();
                found = true;
            }
        }
        if (!found)
        {
            Debug.LogWarning("No Animate with ID \"" + _id + "\" found on GameObject \"" + gameObject.name + "\"");
        }
    }
}
