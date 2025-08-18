using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class Player : Area2D
{
	[Signal]
	public delegate void HitEventHandler();

	[Export]
	public int Speed {get; set;} = 400;  // How fast the player will move (pixels/sec).
	public Vector2 ScreenSize;  // Size of the game window.

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
		Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var velocity = Vector2.Zero;  // The player's movement vector.

		if (Input.IsActionPressed("move_right"))
		{
			velocity.X += 1;
		}

		if (Input.IsActionPressed("move_left"))
		{
			velocity.X -= 1;
		}

		if (Input.IsActionPressed("move_up"))
		{
			velocity.Y -= 1;
		}

		if (Input.IsActionPressed("move_down"))
		{
			velocity.Y += 1;
		}

		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		if (velocity.Length() > 0)
		{
			velocity = velocity.Normalized() * Speed;
			animatedSprite2D.Play();
		}
		else
		{
			animatedSprite2D.Stop();
		}

		Position += velocity * (float)delta;
		Position = new Vector2
		(
			x: Mathf.Clamp(Position.X, 0, ScreenSize.X),
			y: Mathf.Clamp(Position.Y, 0, ScreenSize.Y)
		);

		if (velocity.X != 0)
		{
			animatedSprite2D.Animation = "Walk";
			animatedSprite2D.FlipV = false;
			// Flips the sprite Horizontally
			//animatedSprite2D.FlipH = velocity.X < 0;  // Flip to left if less than 0. 
			
			// Flips the sprite Horizontally, in a different way. More explicit.
			if (velocity.X < 0)
			{
				animatedSprite2D.FlipH = true;
			}
			else 
			{
				animatedSprite2D.FlipH = false;
			}
		}
		else if (velocity.Y != 0)
		{
			animatedSprite2D.Animation = "Up";
			animatedSprite2D.FlipV = velocity.Y > 0;
		}
	}

	// Collision detection
	private void OnBodyEntered(Node2D body)
	{
		Hide();  // Player disappears after being hit.
		EmitSignal(SignalName.Hit);

		// Must be deferred as we can't change physics properties on a physics callback.
		// Each time an enemy hits the player, the signal is going to be emitted. We need to disable the player's collision so that
		// we don't trigger the hit signal more than once.

		// Disabling the area's collision shape can cause an error if it happens in the middle of the engine's collision processing.
		// Using SetDeferred() tells Godot to wait to disable the shape until it's safe to do so.
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	public void Start(Vector2 position)
	{
		Position = position;
		Show();
		GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
	}
}
