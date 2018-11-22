using System.Collections.Generic;
using SmileyBot.ApplicationCore.Models;

namespace SmileyBot.ApplicationCore.Services
{
    public static class BallPredictionService
    {
	public static List<LocationPrediction> GetPredictions(BallWrapper ball, FieldService field, GameWrapper game)
	{
	    var predictions = new List<LocationPrediction>
	    {
		new LocationPrediction
		{
		    DeltaTime = 0,
		    GameTime = game.TimeElapsed,
		    Location = new Vec3(ball.Location.X, ball.Location.Y, ball.Location.Z),
		    Velocity = new Vec3(ball.Velocity.X, ball.Velocity.Y, ball.Velocity.Z)
		}
	    };

	    var timeSpan = .1;
	    for (var i = 1; i < 10 ; i++)
	    {
		var lastPrediction = predictions[i-1];
		var newPrediction = GetPrediction(lastPrediction, field, game, (float)(i * timeSpan));
		predictions.Add(newPrediction);
	    }

	    
	    return predictions;
	}

	private static LocationPrediction GetPrediction(LocationPrediction ball, FieldService field, GameWrapper game, float deltaTime)
	{
	    var prediction = new LocationPrediction();
	    prediction.DeltaTime = deltaTime;
	    prediction.GameTime = game.TimeElapsed + deltaTime;
	    prediction.Location = new Vec3(ball.Location.X, ball.Location.Y, ball.Location.Z);
	    prediction.Velocity = new Vec3 (ball.Velocity.X, ball.Velocity.Y, ball.Velocity.Z);

	    // Apply forces
	    prediction.Location.X += (ball.Velocity.X * deltaTime);
	    prediction.Location.Y += (ball.Velocity.Y * deltaTime);
	    prediction.Location.Z += (ball.Velocity.Z * deltaTime) - (GameValuesService.Gravity * deltaTime);
	    if (prediction.Location.Z < GameValuesService.BallRadius) prediction.Location.Z = GameValuesService.BallRadius;
	    
	    return prediction;
	}
    }
}
