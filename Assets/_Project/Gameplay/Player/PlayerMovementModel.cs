using System;

public sealed class PlayerMovementModel
{
    private const float InputEpsilon = 0.0001f;

    private readonly float _speed;
    private readonly float _smooth;
    private readonly float _minimumX;
    private readonly float _maximumX;
    private readonly float _startX;

    public PlayerMovementModel(
        float startX,
        float speed,
        float smooth,
        float edgePadding,
        IScreenBounds screenBounds)
    {
        if (screenBounds == null)
            throw new ArgumentNullException(nameof(screenBounds));

        _startX = startX;
        _speed = Math.Max(0f, speed);
        _smooth = Math.Max(0f, smooth);
        _minimumX = screenBounds.Left + Math.Max(0f, edgePadding);
        _maximumX = screenBounds.Right - Math.Max(0f, edgePadding);
        PositionX = FloatMath.ClampOrMidpoint(startX, _minimumX, _maximumX);
    }

    public float PositionX { get; private set; }
    public float MovementInput { get; private set; }

    public void Move(float inputX, float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            if (Math.Abs(inputX) <= InputEpsilon)
                MovementInput = 0f;

            return;
        }

        float interpolation = FloatMath.Clamp01(_smooth * deltaTime);
        MovementInput += (inputX - MovementInput) * interpolation;
        PositionX = FloatMath.ClampOrMidpoint(
            PositionX + MovementInput * _speed * deltaTime,
            _minimumX,
            _maximumX);
    }

    public void MoveByNormalizedScreenDeltaX(float normalizedDeltaX)
    {
        float movementDeltaX = normalizedDeltaX * (_maximumX - _minimumX);
        PositionX = FloatMath.ClampOrMidpoint(
            PositionX + movementDeltaX,
            _minimumX,
            _maximumX);
        MovementInput = Math.Abs(movementDeltaX) <= InputEpsilon
            ? 0f
            : Math.Sign(movementDeltaX);
    }

    public void Stop()
    {
        MovementInput = 0f;
    }

    public void Reset()
    {
        MovementInput = 0f;
        PositionX = FloatMath.ClampOrMidpoint(_startX, _minimumX, _maximumX);
    }
}
