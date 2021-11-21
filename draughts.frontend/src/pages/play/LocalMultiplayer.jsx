import React from "react";
import { Redirect } from "react-router";

import MessageBox from "../../components/MessageBox";

export default class LocalMultiplayer extends React.Component {
	constructor(props) {
		super(props);
		this.state = {
			redirect: null
		};
	}

	render() {
		if (this.state.redirect) {
			return (
				<Redirect to={this.state.redirect} />
			);
		}

		return (
			<div className="container">
				<MessageBox title="Creating game..." load={true} />
			</div>
		);
	}

	async componentDidMount() {
		var code = await window._connection.invoke("CREATE_GAME", { gameType: 0 });
		this.setState({ redirect: `/game/${code}` });
	}
}
