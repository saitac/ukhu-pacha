using Godot;
using System;

public partial class Inca : CharacterBody2D
{
	private float _speed = 300;
	private AnimatedSprite2D _incaSprite;
	private Combustible _combustible;
	private Antorcha _antorcha;
	private Direccion _incaUltimaDireccion = Direccion.Abajo;

	public override void _Ready()
	{
		_incaSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_combustible = GetNode<Combustible>("Combustible");
		_antorcha = GetNode<Antorcha>("antorcha");
		_combustible.NivelCambiado += _antorcha.AjustarLlama;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direccion = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		ActualizaAnimacion(direccion);
		Velocity = direccion * _speed;
		MoveAndSlide();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!OS.IsDebugBuild())
		{
			return;
		}

		if (@event.IsActionPressed("debug_recargar"))
		{
			_combustible.Recargar();
		}
	}

	private void ActualizaAnimacion(Vector2 direccion)
	{
		if (direccion == Vector2.Zero)
		{
			_incaSprite.Play("idle-" + _incaUltimaDireccion.Sufijo());
			_antorcha.Orientar(_incaUltimaDireccion);
			return;
		}

		if(Math.Abs(direccion.X) > Math.Abs(direccion.Y)) // se mueve en el eje X
		{
			_incaUltimaDireccion = direccion.X > 0 ? Direccion.Derecha : Direccion.Izquierda;
		} else
		{
			_incaUltimaDireccion = direccion.Y > 0 ? Direccion.Abajo : Direccion.Arriba;
		}

		_incaSprite.Play("walk-" + _incaUltimaDireccion.Sufijo() + "-inca");
		_antorcha.Orientar(_incaUltimaDireccion);
	}
}
