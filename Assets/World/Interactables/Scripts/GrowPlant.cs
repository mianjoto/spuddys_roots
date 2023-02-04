using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowPlant : MonoBehaviour
{
    [SerializeField] GrowPlantData timedPlantData;
    [SerializeField] Transform growthPointTransform;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite[] growthSprites;
    [SerializeField] BoxCollider2D growthClimbableBoxCollider;
    
    public MagicType MagicTypeRequiredToGrow => timedPlantData.MagicTypeRequiredToGrow;
    
    Coroutine _growCoroutine;
    Coroutine _shrinkCoroutine;

    bool _shrinkAfterGrowing;
    float _growthDuration;
    float _shrinkDuration;
    float _beforeGrowthYScale;
    float _afterGrowthYScale;
    float _timeSinceLastGrowth;
    float _lifetime;
    bool _isGrown;

    void Awake()
    {
        _shrinkAfterGrowing = timedPlantData.DoShrinkAfterGrowth;
        _growthDuration = timedPlantData.GrowthDuration;
        _shrinkDuration = timedPlantData.ShrinkDuration;
        _beforeGrowthYScale = timedPlantData.BeforeGrowthYScale;
        _afterGrowthYScale = timedPlantData.AfterGrowthYScale;
        _lifetime = timedPlantData.StayGrownDuration;
    }

    void Start()
    {
        // Reset growth point to the initial position if not at bottom
        if (growthPointTransform.localScale.y >= _beforeGrowthYScale)
            transform.localScale = new Vector3(transform.localScale.x, _beforeGrowthYScale, transform.localScale.z);
        _isGrown = false;

        // Move the climbable collider to bottom of plant
        var colliderSize = growthClimbableBoxCollider.size.y;
        growthClimbableBoxCollider.offset = new Vector2(growthClimbableBoxCollider.offset.x, colliderSize / 2f);

        if (_growCoroutine != null)
            StopCoroutine(_growCoroutine);
        if (_shrinkCoroutine != null)
            StopCoroutine(_shrinkCoroutine);
    }


    void Update()
    {
        if (!_shrinkAfterGrowing || !_isGrown)
            return;

        ShrinkAfterGrowthLifetime();
    }

    void ShrinkAfterGrowthLifetime()
    {
        _timeSinceLastGrowth += Time.deltaTime;
        if (_timeSinceLastGrowth >= _lifetime)
            TryToShrink();
    }

    public void TryToGrow()
    {
        if (_isGrown)
            return;

        if (_growCoroutine != null)
            StopCoroutine(_growCoroutine);

        _isGrown = true;
        _growCoroutine = StartCoroutine(GrowPlantOverTime());
    }

    private void TryToShrink()
    {
        if (!_isGrown)
            return;

        if (_shrinkCoroutine != null)
            StopCoroutine(_shrinkCoroutine);

        _isGrown = false;
        _shrinkCoroutine = StartCoroutine(ShrinkPlantOverTime());
        _timeSinceLastGrowth = 0f;
    }

    IEnumerator GrowPlantOverTime()
    {
        float timer = 0f;
        int numberOfSprites = growthSprites.Length;
        int currentSpriteIndex = 0;
        float updateSpriteInterval = _growthDuration / numberOfSprites;

        while (timer < _growthDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / _growthDuration);
            float currentYScale = Mathf.Lerp(_beforeGrowthYScale, _afterGrowthYScale, t);
            
            growthClimbableBoxCollider.size = new Vector2(growthClimbableBoxCollider.size.x, currentYScale);
            var colliderSize = growthClimbableBoxCollider.size.y;
            growthClimbableBoxCollider.offset = new Vector2(growthClimbableBoxCollider.offset.x, colliderSize / 2f);

            if (timer >= updateSpriteInterval)
            {
                spriteRenderer.sprite = growthSprites[currentSpriteIndex];
                currentSpriteIndex++;
                updateSpriteInterval += _growthDuration / numberOfSprites;
            }
            yield return null;
        }
    }

    IEnumerator ShrinkPlantOverTime()
    {
        float timer = 0f;
        int numberOfSprites = growthSprites.Length;
        int currentSpriteIndex = numberOfSprites - 1;
        float updateSpriteInterval = _growthDuration / numberOfSprites;

        while (timer < _growthDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / _growthDuration);
            float currentYScale = Mathf.Lerp(_afterGrowthYScale, _beforeGrowthYScale, t);
            
            growthClimbableBoxCollider.size = new Vector2(growthClimbableBoxCollider.size.x, currentYScale);
            var colliderSize = growthClimbableBoxCollider.size.y;
            growthClimbableBoxCollider.offset = new Vector2(growthClimbableBoxCollider.offset.x, colliderSize / 2f);

            if (timer >= updateSpriteInterval)
            {
                spriteRenderer.sprite = growthSprites[currentSpriteIndex];
                currentSpriteIndex--;
                updateSpriteInterval += _growthDuration / numberOfSprites;
            }
            yield return null;
        }
    }

}
