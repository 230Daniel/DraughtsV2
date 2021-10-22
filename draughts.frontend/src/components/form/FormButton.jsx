import React from "react";

import styles from "./Form.module.css";

export default class FormButton extends React.Component {
	render() {
		return (
			<button className={styles.button} type={this.props.type} onClick={(e) => this.onClick(e)}>{this.props.children}</button>
		)
	}

	onClick(e) {
		if (this.props.onClick) {
			this.props.onClick(e);
		}
	}
}
