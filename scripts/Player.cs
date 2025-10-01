using Godot;
using System;

public partial class Player : Area2D
{
	[Signal]
	public delegate void HitEventHandler();

	[Export]
	public int Speed {get; set;} = 400;  // How fast the player will move (pixels/sec).
	public Vector2 ScreenSize;  // Size of the game window.
	
	// Track the current flip state (to flip the player character sprite based on movement)
	private bool isFlippedVertically = false;

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
			//GD.Print(velocity);
			velocity = velocity.Normalized() * Speed;
			animatedSprite2D.Play();

			// Determine which animation to play based on dominant movement direction
			// For diagonal movement, determine which direction is more dominant
			if (Math.Abs(velocity.X) > Math.Abs(velocity.Y))
			{
				// Horizontal movement is dominant
				animatedSprite2D.Animation = "Walk";
				animatedSprite2D.Frame = 1;
				animatedSprite2D.FlipH = velocity.X < 0;
			}
			else
			{
				// Vertical movement is dominant
				animatedSprite2D.Animation = "Up";
				animatedSprite2D.Frame = 1;
				isFlippedVertically = velocity.Y > 0;  // Set the flipped state to "true" when moving vertically.
				animatedSprite2D.FlipH = false; // Reset horizontal flip for vertical movement
			}
		}
		else
		{
			animatedSprite2D.Stop();
		}

		Position += velocity * (float)delta;
		Position = new Vector2
		(
			x: Mathf.Clamp(Position.X, 30, ScreenSize.X - 30),
			y: Mathf.Clamp(Position.Y, 35, ScreenSize.Y - 35)
		);

		animatedSprite2D.FlipV = isFlippedVertically;  // When player not moving, set flipped state to "false".
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
