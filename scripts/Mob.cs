using Godot;

public partial class Mob : RigidBody2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var mobAnimations = GetNode<AnimatedSprite2D>("MobAnimations");
		string[] mobTypes = mobAnimations.SpriteFrames.GetAnimationNames();
		mobAnimations.Play(mobTypes[GD.Randi() % mobTypes.Length]);
	}

	private void DeleteEnemyOnScreenExited()
	{
		QueueFree();
	}
}
