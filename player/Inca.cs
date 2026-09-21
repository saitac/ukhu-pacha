using Godot;
using System;

public partial class Inca : CharacterBody2D
{
	private float _speed = 300;
	private AnimatedSprite2D _incaSprite;
	private AnimatedSprite2D _antorchaSprite;
	private string _incaUltimaDireccion = "down";

    public override void _Ready()
    {
        _incaSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_antorchaSprite = GetNode<AnimatedSprite2D>("antorcha/AnimatedSprite2D");
    }

    public override void _PhysicsProcess(double delta)
    {
		Vector2 direccion = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		ActualizaAnimacion(direccion);
		Velocity = direccion * _speed;
		MoveAndSlide();
    }

	private void ActualizaAnimacion(Vector2 direccion)
	{
		if ( direccion == Vector2.Zero)
		{
			_incaSprite.Play("idle-" + _incaUltimaDireccion);
			_antorchaSprite.Play("antorcha-" + _incaUltimaDireccion);
			return;

		}

		if(Math.Abs(direccion.X) > Math.Abs(direccion.Y)) // se mueve en el eje X
		{
			_incaUltimaDireccion = direccion.X > 0 ? "right" : "left";
		} else
		{
			_incaUltimaDireccion = direccion.Y > 0 ? "down" : "up";
		}

		_incaSprite.Play("walk-" + _incaUltimaDireccion + "-inca");
		_antorchaSprite.Play("antorcha-" + _incaUltimaDireccion);
	}
}
