using UnityEngine;
namespace ZooWorld
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class AnimalActor : MonoBehaviour
    {
        private Rigidbody body;
        private IAnimalMotor motor;
        private Arena arena;
        private ZooSimulation simulation;
        private Transform visual;
        private Vector3 heading;
        private Vector3 propulsion;
        private float nextTurn;
        public AnimalDefinition Definition { get; private set; }
        public int BirthId { get; private set; }
        public bool IsAlive { get; private set; }

        public void Initialize(AnimalDefinition definition, int id, Arena arena, ZooSimulation simulation, Transform visual)
        {
            Definition = definition; BirthId = id; this.arena = arena; this.simulation = simulation;
            this.visual = visual; motor = definition.Movement.CreateMotor();
            body = GetComponent<Rigidbody>(); body.mass = definition.Mass;
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            GetComponent<SphereCollider>().radius = definition.Radius;
            PickHeading(); IsAlive = true;
        }
        private void PickHeading()
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            heading = new Vector3(direction.x, 0, direction.y);
            nextTurn = Random.Range(1.5f, 3.5f);
        }
        private void FixedUpdate()
        {
            if (!IsAlive) return;
            float dt = Time.fixedDeltaTime;
            nextTurn -= dt;
            if (nextTurn <= 0) PickHeading();
            if (arena.GetReturnHeading(body.position, Definition.Radius, out var inward)) heading = inward;
            Vector3 desired = motor.Step(heading, dt);
            // Change only the motor contribution. Preserve collision impulses and damp them gradually,
            // otherwise a stationary frog would erase a physical bounce on the very next tick.
            Vector3 impact = body.linearVelocity - propulsion;
            impact = Vector3.MoveTowards(impact, Vector3.zero, 5f * dt);
            propulsion = Vector3.MoveTowards(propulsion, desired, motor.VelocityResponse * dt);
            body.linearVelocity = propulsion + impact;
            if (desired.sqrMagnitude > 0.001f) visual.rotation = Quaternion.LookRotation(desired);
            visual.localPosition = Vector3.up * motor.VisualHeight;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent<AnimalActor>(out var other)) simulation.Resolve(this, other);
        }
        public bool TryDie()
        {
            if (!IsAlive) return false;
            IsAlive = false;
            GetComponent<Collider>().enabled = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
            return true;
        }
    }
}
