# RabbitMQ and Couchbase/Redis ETL Integration

This project demonstrates an integration between RabbitMQ, Couchbase, and Redis for Extract-Transform-Load (ETL) operations.

## Overview

The project involves several components working together:

- **RabbitMQ**: Message broker responsible for receiving and distributing messages between producers and consumers.
- **Couchbase**: NoSQL database used for storing JSON documents received from RabbitMQ.
- **Redis**: In-memory data store used for caching data retrieved from Couchbase.
- **Rabbit Producer**: Component responsible for generating objects and publishing them to RabbitMQ.
- **Rabbit Consumer**: Component responsible for consuming messages from RabbitMQ and storing them in Couchbase.
- **Redis ETL**: Component responsible for periodically fetching data from Couchbase and storing it in Redis.

## Project Structure

- `RabbitProducer`: Generates objects and publishes them to RabbitMQ.
- `RabbitConsumer`: Consumes messages from RabbitMQ and stores them in Couchbase.
- `RedisETL`: Fetches data from Couchbase and stores it in Redis.

## How It Works

1. **Rabbit Producer**:
   - Generates objects and publishes them to RabbitMQ.
   - Uses a RabbitMQ exchange to route messages to the appropriate queues.

2. **Rabbit Consumer**:
   - Consumes messages from RabbitMQ queues.
   - Parses the messages and stores them in Couchbase.

3. **Redis ETL**:
   - Periodically fetches data from Couchbase.
   - Transforms the data if necessary.
   - Stores the data in Redis for caching purposes.

## Running the Project

1. Ensure you have Docker installed on your machine.
2. Clone this repository.
3. Navigate to the project directory.
4. Run `docker-compose up` to start the services.

## Configuration

- Configuration for RabbitMQ, Couchbase, and Redis can be adjusted in the `docker-compose.yml` file.
- Additional configuration options for individual components can be found in their respective directories (`RabbitProducer`, `RabbitConsumer`, `RedisETL`).

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvement, feel free to open an issue or submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE).
