using Godot;
using System;

public partial class Supay : Enemigo
{
	private enum Estado
	{
		FueraDeEscena,
		Apareciendo,
		Persiguiendo,
		Atacando,
		Desapareciendo
	};

	private Laberinto _laberinto;
	private Inca _inca;
	private DiscoInti _discoInti;
	private Estado _estado = Estado.FueraDeEscena;
	private AnimatedSprite2D _sprite;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.AnimationFinished += OnAnimationFinished;
	}

	public void Inicializar(Laberinto laberinto, Inca inca, DiscoInti discoInti)
	{
		_laberinto = laberinto;
		_inca = inca;
		_discoInti = discoInti;
		_discoInti.Tomado += OnDiscoTomado;
	}

	private void CambiarEstado(Estado nuevo)
	{
		_estado = nuevo;
	}

	private void OnDiscoTomado(Inca portador, Vector2 posicionTomado)
	{
		GlobalPosition = _laberinto.ElegirPuntoEmergencia(posicionTomado);
		CambiarEstado(Estado.Apareciendo);
	}

	private void OnAnimationFinished()
	{
		if (_estado == Estado.Apareciendo)
		{
			if (_discoInti.EstaEnJuego)
			{
				CambiarEstado(Estado.Persiguiendo);
			}
			else
			{
				CambiarEstado(Estado.Desapareciendo);
			}
		}
	}

}
