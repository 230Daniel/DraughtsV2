import React from "react";
import { BrowserRouter as Router, Switch, Route } from "react-router-dom";
import { HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";

import Footer from "./components/layout/Footer";
import Navbar from "./components/layout/Navbar";
import Index from "./pages/Index";
import Play from "./pages/Play";
import Game from "./pages/Game";

import MessageBox from "./components/MessageBox";

export default class App extends React.Component{

	constructor(props){
		super(props);
		this.state = {
			status: "Connecting to the server..."
		}
	}

	render(){
		return(
			<Router>
				<main>
					<Navbar/>
					{this.renderContent()}
				</main>
				<footer>
					<Footer/>
				</footer>
			</Router>
		)
	}

	renderContent(){
		if (this.state.status === "Ready") {
			return(
				<Switch>
					<Route exact path="/" component={Index}/>
					<Route exact path="/play" component={Play}/>
					<Route exact path="/game" component={Game}/>
				</Switch>
			);
		}

		if(this.state.status === "Error"){
			return(
				<div className="container">
					<MessageBox title="Error" message={this.state.error}/>
				</div>
			);
		}

		return(
			<div className="container">
				<MessageBox title={this.state.status} load={true}/>
			</div>
		);
	}

	async componentDidMount(){
		await this.fetchAntiforgeryToken();
		await this.connectHub();
		this.setState({status: "Ready"});
	}

	async componentWillUnmount(){
		window._connection?.stop();
		window._connection = undefined;
	}

	async fetchAntiforgeryToken(){
		try {
			var response = await fetch(`${window._config.backend}/antiforgery`);
			window._antiForgeryToken = await response.json();
		} catch(ex) {
			this.setState({status: "Error", error: "Failed to fetch the anti-forgery token"});
			throw ex;
		}
	}

	async connectHub(){
		try {
			var connection = new HubConnectionBuilder()
				.withUrl(`${window._config.backend}/hub`, { headers: { "X-XSRF-TOKEN": window._antiForgeryToken, credentials: 'exclude' }})
				.withAutomaticReconnect()
				.build();

			connection.onreconnecting((error) => this.onHubReconnecting(error));
			connection.onreconnected((error) => this.onHubReconnected());

			await connection.start();
			window._connection = connection;
		} catch(ex) {
			this.setState({status: "Error", error: "Failed to connect to the hub"});
			throw ex;
		}
	}

	async onHubReconnecting(error){
		setTimeout(() => {
			if(window._connection.state !== HubConnectionState.Connected)
				this.setState({status: "Reconnecting to the server...", error: error ? error.toString() : null});
		}, 1000);
	}

	async onHubReconnected(){
		this.setState({status: "Ready"});
	}
}
