import React from "react";
import { BrowserRouter as Router, Switch, Route } from "react-router-dom";
import { HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";

import Footer from "./components/layout/Footer";
import Navbar from "./components/layout/Navbar";
import Index from "./pages/Index";
import HowToPlay from "./pages/HowToPlay";
import Play from "./pages/play/Play";
import Game from "./pages/Game";

import MessageBox from "./components/MessageBox";
import ScrollToTop from "./components/ScrollToTop";

export default class App extends React.Component {

	constructor(props) {
		super(props);
		this.state = {
			status: "Connecting to the server..."
		};
	}

	// Render the main structure of the page including the navbar, content and footer
	render() {
		return (
			<Router>
				<main>
					<Navbar />
					{this.renderContent()}
					<ScrollToTop />
				</main>
				<footer>
					<Footer />
				</footer>
			</Router>
		);
	}

	renderContent() {
		// If everything is connected, render the page requested by the URL
		if (this.state.status === "Ready") {
			return (
				<Switch>
					<Route exact path="/" component={Index} />
					<Route exact path="/how-to-play" component={HowToPlay} />
					<Route path="/play" component={Play} />
					<Route exact path="/game/:gameCode" component={Game} />
				</Switch>
			);
		}

		// If something went wrong display an error message
		if (this.state.status === "Error") {
			return (
				<div className="container">
					<MessageBox title="Error" message={this.state.error} />
				</div>
			);
		}

		// If we're still connecting display a loading message
		return (
			<div className="container">
				<MessageBox title={this.state.status} load={true} />
			</div>
		);
	}

	async componentDidMount() {
		// When the website is opened connect to the server
		await this.fetchAntiforgeryToken();
		await this.connectHub();

		// If all goes well proceed to the main site
		this.setState({ status: "Ready" });
	}

	async componentWillUnmount() {
		// When the website is unloaded stop the connection
		window._connection?.stop();
	}

	async fetchAntiforgeryToken() {
		// An anti-forgery token must be fetched and then used in subsequent requests to protect the server against XSRF attacks
		try {
			var response = await fetch(`${window._config.backend}/antiforgery`);
			window._antiForgeryToken = await response.json();
		} catch (ex) {
			this.setState({ status: "Error", error: "Failed to fetch the anti-forgery token" });
			throw ex;
		}
	}

	async connectHub() {
		try {
			// Connect to the SignalR hub of the server for 2-way communication
			var connection = new HubConnectionBuilder()
				.withUrl(`${window._config.backend}/hub`, { headers: { "X-XSRF-TOKEN": window._antiForgeryToken } })
				.withAutomaticReconnect()
				.build();

			// Register reconnecting and reconnected events to display loading messages while this occurs
			connection.onreconnecting((error) => this.onHubReconnecting(error));
			connection.onreconnected(() => this.onHubReconnected());

			// Start the connection and set the value on the window for other components to use
			await connection.start();
			window._connection = connection;
		} catch (ex) {
			this.setState({ status: "Error", error: "Failed to connect to the hub" });
			throw ex;
		}
	}

	async onHubReconnecting(error) {
		// If the hub hasn't reconnected after 1 second display a loading message
		setTimeout(() => {
			if (window._connection.state !== HubConnectionState.Connected)
				this.setState({ status: "Reconnecting to the server...", error: error ? error.toString() : null });
		}, 1000);
	}

	async onHubReconnected() {
		// The hub has reconnected so return the user to the site
		this.setState({ status: "Ready" });
	}
}
